using System.Collections.Concurrent;
using Persistence.DocumentRepository;
using YDotNet.Document;
using YDotNet.Document.Transactions;
using YDotNet.Protocol;

namespace ApplicationLogic;

public class UpdatesLogic {

    private DocumentRepositoryCassandra _docRepoCass;
    private DocumentRepositoryRedis _docRepoRed;

    private static readonly ConcurrentDictionary<(Guid, Guid), List<byte[]>> _docs = [];

    public UpdatesLogic(DocumentRepositoryCassandra docRepoCass, DocumentRepositoryRedis docRepoRed) {
        _docRepoCass = docRepoCass;
        _docRepoRed = docRepoRed;
    }

    /*
    
    CREATE TABLE documents (
        workspace_id UUID,
        document_id UUID,
        document_name TEXT,
        created_at TIMESTAMP,
        creator_user_id UUID,
        snapshot1 TIMESTAMP,
        PRIMARY KEY ((workspace_id, document_id))
    );
    
    */

    public bool VerifyExists(Guid workspaceId, Guid docId) {
        // return _docRepoCass.GetDocument(workspaceId, docId) != null;
        if (!_docs.ContainsKey((workspaceId, docId))) {
            _docs[(workspaceId, docId)] = [];
        }
        return true;
    }

    public byte[] GetDocumentBytes(Guid workspaceId, Guid docId, byte[] stateVector) {
        Doc doc = new();
        Console.Write("Here: ");
        Console.WriteLine(_docs[(workspaceId, docId)].Count());
        foreach (var update in _docs[(workspaceId, docId)]) {
            Console.WriteLine("Update");
            Transaction writeTransaction = doc.WriteTransaction();
            writeTransaction.ApplyV1(update);
            writeTransaction.Commit();
        }
        Transaction readTransaction = doc.ReadTransaction();
        var bytes = readTransaction.StateDiffV1(stateVector);
        readTransaction.Commit();

        return bytes;
    }

    public void UpdateDoc(Guid workspaceId, Guid docId, SyncUpdateMessage updateMsg) {
        // ulong update_id = BitConverter.ToUInt64(updateMsg.Update); // first 8 byte
        _docs[(workspaceId, docId)].Add(updateMsg.Update);
    }

}


public class UpdateIdentifier {
    public uint ClientId { get; set; }
    public uint Clock { get; set; }
}
