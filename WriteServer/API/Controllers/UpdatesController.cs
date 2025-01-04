using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using ApplicationLogic;
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

        await RecieveMessageAsync(conn, sharedDoc);
    }

    class RealTimeUpdate {
        public required string Update { get; set; }
    }

    private async Task RecieveMessageAsync(WebSocket conn, SharedDoc doc) {
        var encoder = new WebSocketEncoder(conn);
        var decoder = new WebSocketDecoder(conn);
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var clientPort = HttpContext.Connection.RemotePort.ToString();



        var subscriber = RedisSessionManager.GetSubscriber();
        await subscriber.SubscribeAsync(new RedisChannel($"realtimeupdate-{doc.DocumentId}", RedisChannel.PatternMode.Literal),
            async (redisChannel, message) => {
                var deserialized = JsonSerializer.Deserialize<RealTimeUpdate>(message!);
                byte[] update_bytes = Convert.FromBase64String(deserialized!.Update);
                foreach (var sock in doc.Conns) {
                    if (sock != conn) {
                        var foreignEncoder = new WebSocketEncoder(sock);
                        await foreignEncoder.WriteAsync(new SyncUpdateMessage(update_bytes), CancellationToken.None);
                    }
                }
            });

        while (true) {
            try {
                var msg = await decoder.ReadNextMessageAsync(CancellationToken.None);

                if (msg is SyncStep1Message msg1) {
                    byte[] documentState = await _updatesLogic.GetDocumentBytes(doc.DocumentId, doc.SnapshotId, msg1.StateVector);

                    await encoder.WriteAsync(new SyncStep2Message(documentState), CancellationToken.None);
                }
                else if (msg is SyncUpdateMessage msg2) {
                    _updatesLogic.UpdateDoc(doc.WorkspaceId, doc.DocumentId, msg2);

                    foreach (var sock in doc.Conns) {
                        if (sock != conn) {
                            var foreignEncoder = new WebSocketEncoder(sock);
                            await foreignEncoder.WriteAsync(msg2, CancellationToken.None);
                        }
                    }
                }
                else if (msg is AwarenessMessage msg3) {
                    foreach (var sock in doc.Conns) {
                        if (sock != conn) {
                            var foreignEncoder = new WebSocketEncoder(sock);
                            await foreignEncoder.WriteAsync(msg3, CancellationToken.None);
                        }
                    }
                }

            }
            catch (WebSocketException) {
                CloseConnection(doc, conn);
                _logger.LogInformation("Closing connection from {}:{}", clientIp, clientPort);
                break;
            }
            catch (CantLoadDocumentContentException) {
                CloseConnection(doc, conn);
                break;
            }
            catch (FailedToQueueUpdateException) {
                CloseConnection(doc, conn);
                break;
            }
        }
    }

    private static void CloseConnection(SharedDoc doc, WebSocket conn) {
        doc.Conns.Remove(conn);

        if (doc.Conns.Count == 0) {
            docs.TryRemove((doc.WorkspaceId, doc.DocumentId, doc.SnapshotId), out var _);
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