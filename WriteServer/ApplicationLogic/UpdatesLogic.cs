using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Persistence.DocumentRepository;
using YDotNet.Document;
using YDotNet.Document.Transactions;
using YDotNet.Protocol;

namespace ApplicationLogic;

public class UpdatesLogic {

    private DocumentRepositoryCassandra _docRepoCass;
    private DocumentRepositoryRedis _docRepoRed;
    private readonly ILogger<UpdatesLogic> _logger;
    private static readonly ConcurrentDictionary<(Guid, Guid), List<byte[]>> _docs = [];

    public UpdatesLogic(DocumentRepositoryCassandra docRepoCass, DocumentRepositoryRedis docRepoRed, ILogger<UpdatesLogic> logger) {
        _docRepoCass = docRepoCass;
        _docRepoRed = docRepoRed;
        _logger = logger;
    }

    public bool VerifyExists(Guid workspaceId, Guid docId) {
        if (!_docs.ContainsKey((workspaceId, docId))) {
            _docs[(workspaceId, docId)] = [];
        }
        return true;
    }

    public async Task<byte[]> GetDocumentBytes(Guid docId, byte[] stateVector) {
        try {
            List<byte[]> bytes = await _docRepoCass.GetSnapshot(docId, "snapshot1");
            _logger.LogInformation("Successfully aquired document bytes for document {}", docId);

            Doc doc = new();
            foreach (var update in bytes) {
                Transaction readTransaction = doc.ReadTransaction();
                readTransaction.ApplyV1(update);
                readTransaction.Commit();
            }

            Transaction readTransaction1 = doc.ReadTransaction();
            var mergedUpdates = readTransaction1.StateDiffV1(stateVector);
            readTransaction1.Commit();

            return mergedUpdates;
        }
        catch (Exception ec) {
            _logger.LogError("Failed to aquire document bytes for document {} due to {}", docId, ec.Message);
            throw new 
        } 
    }

    public async void UpdateDoc(Guid workspaceId, Guid docId, SyncUpdateMessage updateMsg) {
        try {
            await _docRepoRed.SaveUpdateAsync(docId, updateMsg.Update);
            _logger.LogInformation("Update to {}:{} successfully written to message queue", workspaceId, docId);
        }
        catch (Exception ec) {
            _logger.LogError("Update to {}:{} could not be written to message queue due to {}", workspaceId, docId, ec.Message);
        }
        _docs[(workspaceId, docId)].Add(updateMsg.Update);
    }

}
