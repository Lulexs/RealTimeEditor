using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Text.Json;
using ApplicationLogic;
using ApplicationLogic.Dtos;
using ApplicationLogic.Exceptions;
using Persistence;
using StackExchange.Redis;
using YDotNet.Protocol;
using YDotNet.Server.WebSockets;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class UpdatesController : ControllerBase {

    private readonly UpdatesLogic _updatesLogic;
    private static readonly ConcurrentDictionary<(Guid, Guid, string), SharedDoc> docs = [];
    private readonly ILogger<UpdatesController> _logger;
    private readonly ISubscriber interWriteServerSub = RedisSessionManager.GetSubscriber();

    public UpdatesController(UpdatesLogic updatesLogic, ILogger<UpdatesController> logger) {
        _updatesLogic = updatesLogic;
        _logger = logger;
    }

    [Route("/ws/{workspaceGuid}/{documentGuid}/{snapshotId}")]
    public async Task Get(string workspaceGuid, string documentGuid, string snapshotId) {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var clientPort = HttpContext.Connection.RemotePort.ToString();
        _logger.LogInformation("Incoming WebSocket request from {ClientIp}:{ClientPort}", clientIp, clientPort);

        if (!Guid.TryParse(workspaceGuid, out var workspaceId) || !Guid.TryParse(documentGuid, out var documentId)) {
            await CloseWithErrorAsync(
                WebSocketCloseStatus.InvalidPayloadData,
                "Invalid GUID format",
                $"Rejected WebSocket connection from {clientIp}:{clientPort} for workspace {workspaceGuid} and document {documentGuid}/{snapshotId} as id is not guid"
            );
            return;
        }

        if (!HttpContext.WebSockets.IsWebSocketRequest) {
            await CloseWithErrorAsync(
                WebSocketCloseStatus.ProtocolError,
                "HTTP endpoint requires WebSocket protocol",
                $"Rejected request from {clientIp}:{clientPort} as it is not a WebSocket request"
            );
            return;
        }

        if (!await VerifyRequestParams(workspaceId, documentId)) {
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        _logger.LogInformation("Accepted WebSocket connection from {ClientIp}:{ClientPort} for workspace {WorkspaceId} and document {DocumentId}/{SnapshotId}",
            clientIp, clientPort, workspaceId, documentId, snapshotId);

        await SetupWSConnection(webSocket, workspaceId, documentId, snapshotId);
    }

    private async Task<bool> VerifyRequestParams(Guid workspaceId, Guid documentId) {
        var docExists = await _updatesLogic.VerifyExistsAsync(workspaceId, documentId);
        if (!docExists) {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var clientPort = HttpContext.Connection.RemotePort.ToString();

            await CloseWithErrorAsync(
                WebSocketCloseStatus.InvalidPayloadData,
                "Document doesn't exist",
                $"Requested document (workspaceId: {workspaceId}, documentId: {documentId}) from {clientIp}:{clientPort} doesn't exist"
            );
            return false;
        }
        return true;
    }

    private async Task CloseWithErrorAsync(WebSocketCloseStatus status, string reason, string logMessage) {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await ws.CloseAsync(status, reason, CancellationToken.None);
        }
        else {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsync(reason);
        }

        _logger.LogInformation("{}", logMessage);
    }

    private async Task SetupWSConnection(WebSocket conn, Guid workspaceId, Guid documentId, string snapshotId) {
        if (docs.TryGetValue((workspaceId, documentId, snapshotId), out SharedDoc? sharedDoc)) {
            sharedDoc.Conns.Add(conn);
        }
        else {
            sharedDoc = new SharedDoc(workspaceId, documentId, snapshotId);
            sharedDoc.Conns.Add(conn);
            docs.TryAdd((workspaceId, documentId, snapshotId), sharedDoc);
        }

        var keepAliveCts = new CancellationTokenSource();
        _ = Task.Run(() => KeepAlive(conn, keepAliveCts.Token));

        try {
            await RecieveMessageAsync(conn, sharedDoc);
        }
        finally {
            keepAliveCts.Cancel();
        }
    }

    private async Task RecieveMessageAsync(WebSocket conn, SharedDoc doc) {
        var encoder = new WebSocketEncoder(conn);
        var decoder = new WebSocketDecoder(conn);
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var clientPort = HttpContext.Connection.RemotePort.ToString();

        await interWriteServerSub.SubscribeAsync(new RedisChannel("realtimeupdate-*", RedisChannel.PatternMode.Pattern),
            async (channel, message) => {
                if (channel.ToString() == $"realtimeupdate-{doc.DocumentId}" && !message.IsNullOrEmpty) {
                    var deserializedMessage = JsonSerializer.Deserialize<UpdateDto>(message!);
                    if (deserializedMessage == null)
                        return;

                    byte[] update_bytes = Convert.FromBase64String(deserializedMessage.Update);
                    await SafeSendAsync(conn, new SyncUpdateMessage(update_bytes), _logger);
                }
            });

        await interWriteServerSub.SubscribeAsync(new RedisChannel("awareness-*", RedisChannel.PatternMode.Pattern),
            async (channel, message) => {
                if (channel.ToString() == $"awareness-{doc.DocumentId}" && !message.IsNullOrEmpty) {
                    byte[] msg = (byte[])message!;

                    int num = BitConverter.ToInt32(msg);

                    byte[] awarenessBytes = new byte[msg.Length - sizeof(int)];
                    msg.Skip(sizeof(int)).ToArray().CopyTo(awarenessBytes, 0);
                    AwarenessMessage? msg3 = JsonSerializer.Deserialize<AwarenessMessage>(awarenessBytes);
                    if (msg3 == null)
                        return;

                    if (GetHashCode() == num) {
                        foreach (var sock in doc.Conns) {
                            if (sock != conn) {
                                await SafeSendAsync(sock, msg3!, _logger);
                            }
                        }
                    }
                    else {
                        await SafeSendAsync(conn, msg3, _logger);
                    }
                }
            });

        while (true) {
            try {
                var ct = HttpContext.RequestAborted;
                var msg = await decoder.ReadNextMessageAsync(ct);

                if (msg is SyncStep1Message msg1) {
                    byte[] documentState = await _updatesLogic.GetDocumentBytes(doc.DocumentId, doc.SnapshotId, msg1.StateVector);

                    await SafeSendAsync(conn, new SyncStep2Message(documentState), _logger);
                }
                else if (msg is SyncUpdateMessage msg2) {
                    _updatesLogic.UpdateDoc(doc.WorkspaceId, doc.DocumentId, msg2);

                    foreach (var sock in doc.Conns) {
                        if (sock != conn) {
                            await SafeSendAsync(sock, msg2, _logger);
                        }
                    }
                }
                else if (msg is AwarenessMessage msg3) {

                    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(msg3);
                    int num = GetHashCode();
                    byte[] msgBytes = new byte[sizeof(int) + bytes.Length];
                    BitConverter.GetBytes(num).CopyTo(msgBytes, 0);
                    bytes.CopyTo(msgBytes, sizeof(int));

                    await interWriteServerSub.PublishAsync(new RedisChannel($"awareness-{doc.DocumentId}", RedisChannel.PatternMode.Literal), msgBytes);
                }

            }
            catch (Exception e) when (e is WebSocketException ||
                                      e is CantLoadDocumentContentException ||
                                      e is FailedToQueueUpdateException ||
                                      e is OperationCanceledException) {
                CloseConnection(doc, conn);
                _logger.LogInformation("Closing connection from {ClientIp}:{ClientPort} due to {Exception}",
                    clientIp, clientPort, e.GetType().Name);
                break;
            }
            catch (Exception e) {
                _logger.LogError(e, "Unexpected error in WebSocket connection from {ClientIp}:{ClientPort}",
                    clientIp, clientPort);
                CloseConnection(doc, conn);
                break;
            }
        }
    }

    private void CloseConnection(SharedDoc doc, WebSocket conn) {
        doc.Conns.Remove(conn);

        if (doc.Conns.Count == 0) {
            docs.TryRemove((doc.WorkspaceId, doc.DocumentId, doc.SnapshotId), out var _);
        }

        if (doc.Conns.Count == 0) {
            docs.TryRemove((doc.WorkspaceId, doc.DocumentId, doc.SnapshotId), out _);
            interWriteServerSub.Unsubscribe(new RedisChannel($"realtimeupdate-{doc.DocumentId}", RedisChannel.PatternMode.Literal));
            interWriteServerSub.Unsubscribe(new RedisChannel($"awareness-{doc.DocumentId}", RedisChannel.PatternMode.Literal));
        }

    }

    private async Task SafeSendAsync(WebSocket socket, BaseMessage msg, ILogger logger) {
        if (socket.State != WebSocketState.Open) return;

        try {
            var encoder = new WebSocketEncoder(socket);
            if (msg is SyncStep1Message syncStep1Message)
                await encoder.WriteAsync(syncStep1Message, CancellationToken.None);
            else if (msg is SyncStep2Message syncStep2Message)
                await encoder.WriteAsync(syncStep2Message, CancellationToken.None);
            else if (msg is AwarenessMessage awarenessMessage)
                await encoder.WriteAsync(awarenessMessage, CancellationToken.None);
            else if (msg is SyncUpdateMessage updateMessage)
                await encoder.WriteAsync(updateMessage, CancellationToken.None);

        }
        catch (ObjectDisposedException) {
            logger.LogDebug("Tried to send on disposed socket");
        }
        catch (WebSocketException ex) {
            logger.LogDebug(ex, "WebSocket send failed (probably closed)");
        }
        catch (Exception ex) {
            logger.LogError(ex, "Unexpected error while sending WebSocket message");
        }
    }

    private async Task KeepAlive(WebSocket socket, CancellationToken ct) {
        while (!ct.IsCancellationRequested && socket.State == WebSocketState.Open) {
            await socket.SendAsync(
                new ArraySegment<byte>(Array.Empty<byte>()),
                WebSocketMessageType.Binary,
                true,
                ct
            );
            await Task.Delay(TimeSpan.FromSeconds(30), ct);
        }
    }

    private class SharedDoc {
        public Guid WorkspaceId { get; set; }
        public Guid DocumentId { get; set; }
        public string SnapshotId { get; set; }
        public HashSet<WebSocket> Conns = [];

        public SharedDoc(Guid workspaceId, Guid documentId, string snapshotId) {
            WorkspaceId = workspaceId;
            DocumentId = documentId;
            SnapshotId = snapshotId;
        }

    }
}