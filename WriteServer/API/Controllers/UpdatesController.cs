using System.Collections.Concurrent;
using System.Net.WebSockets;
using ApplicationLogic;
using YDotNet.Protocol;
using YDotNet.Server.WebSockets;

namespace API.Controllers;

public class UpdatesController : BaseController {

    private readonly UpdatesLogic _updatesLogic;
    private static readonly ConcurrentDictionary<(Guid, Guid), SharedDoc> docs = [];

    public UpdatesController(UpdatesLogic updatesLogic) {
        _updatesLogic = updatesLogic;
    }

    [Route("/ws/{workspaceGuid}/{documentGuid}")]
    public async Task Get(string workspaceGuid, string documentGuid) {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            Console.WriteLine(workspaceGuid);
            Console.WriteLine(documentGuid);

            if (!Guid.TryParse(workspaceGuid, out var workspaceId) || !Guid.TryParse(documentGuid, out var documentId)) {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsync("Invalid GUID format");
                return;
            }

            await SetupWSConnection(webSocket, workspaceId, documentId);
        }
        else {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task SetupWSConnection(WebSocket conn, Guid workspaceId, Guid documentId) {
        var docExists = _updatesLogic.VerifyExists(workspaceId, documentId);
        SharedDoc? sharedDoc;
        if (!docExists) {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsync("Document doesn't exist");
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

        while (true) {
            try {
                var msg = await decoder.ReadNextMessageAsync(CancellationToken.None);

                if (msg is SyncStep1Message msg1) {
                    byte[] documentState = _updatesLogic.GetDocumentBytes(doc.WorkspaceId, doc.DocumentId, msg1.StateVector);

                    await encoder.WriteAsync(new SyncStep2Message(documentState), CancellationToken.None);
                }
                else if (msg is SyncUpdateMessage msg2) {
                    // WRITE TO PUBSUB
                    // Transaction writeTransaction = doc.WriteTransaction();
                    // writeTransaction.ApplyV1(msg2.Update);
                    // writeTransaction.Commit();
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
                break;
            }
        }
    }

    private static void CloseConnection(SharedDoc doc, WebSocket conn) {
        Console.WriteLine("Closing connection");
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