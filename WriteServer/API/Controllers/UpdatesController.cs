using System.Collections.Concurrent;
using System.Net.WebSockets;
using ApplicationLogic;
using ApplicationLogic.Exceptions;
using YDotNet.Protocol;
using YDotNet.Server.WebSockets;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class UpdatesController : ControllerBase {

    private readonly UpdatesLogic _updatesLogic;
    private static readonly ConcurrentDictionary<(Guid, Guid), SharedDoc> docs = [];
    private readonly ILogger<UpdatesController> _logger;

    public UpdatesController(UpdatesLogic updatesLogic, ILogger<UpdatesController> logger) {
        _updatesLogic = updatesLogic;
        _logger = logger;
    }

    [Route("/ws/{workspaceGuid}/{documentGuid}")]
    public async Task Get(string workspaceGuid, string documentGuid) {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var clientPort = HttpContext.Connection.RemotePort.ToString();
        _logger.LogInformation("Incoming WebSocket request from {ClientIp}:{ClientPort}", clientIp, clientPort);

        if (!Guid.TryParse(workspaceGuid, out var workspaceId) || !Guid.TryParse(documentGuid, out var documentId)) {
            await CloseWithErrorAsync(
                WebSocketCloseStatus.InvalidPayloadData,
                "Invalid GUID format",
                $"Rejected WebSocket connection from {clientIp}:{clientPort} for workspace {workspaceGuid} and document {documentGuid} as id is not guid"
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
        _logger.LogInformation("Accepted WebSocket connection from {ClientIp}:{ClientPort} for workspace {WorkspaceId} and document {DocumentId}",
            clientIp, clientPort, workspaceId, documentId);

        await SetupWSConnection(webSocket, workspaceId, documentId);
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

        _logger.LogInformation(logMessage);
    }

    private async Task SetupWSConnection(WebSocket conn, Guid workspaceId, Guid documentId) {

        if (docs.TryGetValue((workspaceId, documentId), out SharedDoc? sharedDoc)) {
            sharedDoc.Conns.Add(conn);
        }
        else {
            sharedDoc = new SharedDoc(workspaceId, documentId);
            sharedDoc.Conns.Add(conn);

            docs.TryAdd((workspaceId, documentId), sharedDoc);
        }

        await RecieveMessageAsync(conn, sharedDoc);
    }

    private async Task RecieveMessageAsync(WebSocket conn, SharedDoc doc) {
        var encoder = new WebSocketEncoder(conn);
        var decoder = new WebSocketDecoder(conn);
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var clientPort = HttpContext.Connection.RemotePort.ToString();

        while (true) {
            try {
                var msg = await decoder.ReadNextMessageAsync(CancellationToken.None);

                if (msg is SyncStep1Message msg1) {
                    byte[] documentState = await _updatesLogic.GetDocumentBytes(doc.DocumentId, msg1.StateVector);

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
            docs.TryRemove((doc.WorkspaceId, doc.DocumentId), out var _);
        }
    }

    private class SharedDoc {
        public Guid WorkspaceId { get; set; }
        public Guid DocumentId { get; set; }
        public HashSet<WebSocket> Conns = [];

        public SharedDoc(Guid workspaceId, Guid documentId) {
            WorkspaceId = workspaceId;
            DocumentId = documentId;
        }

    }
}