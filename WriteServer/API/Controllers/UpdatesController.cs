using System.Collections.Concurrent;
using System.Net.WebSockets;
using ApplicationLogic;
using YDotNet.Protocol;
using YDotNet.Server.WebSockets;

namespace API.Controllers;

public class UpdatesController : BaseController {

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

        if (HttpContext.WebSockets.IsWebSocketRequest) {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            _logger.LogInformation("Incoming WebSocket request from {}:{}", clientIp, clientPort);

            if (!Guid.TryParse(workspaceGuid, out var workspaceId) || !Guid.TryParse(documentGuid, out var documentId)) {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsync("Invalid GUID format");
                return;
            }

            _logger.LogInformation("Accepted WebSocket connection from {}:{} for workspace {} and document {}", clientIp, clientPort, workspaceId, documentId);
            await SetupWSConnection(webSocket, workspaceId, documentId);
        }
        else {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            _logger.LogInformation("Rejected request from {}:{} as it is not a WebSocket request", clientIp, clientPort);
        }
    }

    private async Task SetupWSConnection(WebSocket conn, Guid workspaceId, Guid documentId) {
        var docExists = _updatesLogic.VerifyExists(workspaceId, documentId);
        SharedDoc? sharedDoc;
        if (!docExists) {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsync("Document doesn't exist");

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var clientPort = HttpContext.Connection.RemotePort.ToString();
            _logger.LogInformation("Requested document (workspaceId: {}, documentId: {}) from {}:{} doesn't exist", workspaceId, documentId, clientIp, clientPort);
            return;
        }
        else if (docs.TryGetValue((workspaceId, documentId), out sharedDoc)) {
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