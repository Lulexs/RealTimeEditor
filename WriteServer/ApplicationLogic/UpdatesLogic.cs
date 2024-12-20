using ApplicationLogic.Exceptions;
using Microsoft.Extensions.Logging;
using Persistence.DocumentRepository;
using YDotNet.Document;
using YDotNet.Document.Transactions;
using YDotNet.Protocol;

namespace ApplicationLogic;

public class UpdatesLogic {

    private readonly DocumentRepositoryCassandra _docRepoCass;
    private readonly DocumentRepositoryRedis _docRepoRed;
    private readonly ILogger<UpdatesLogic> _logger;

    public UpdatesLogic(DocumentRepositoryCassandra docRepoCass, DocumentRepositoryRedis docRepoRed, ILogger<UpdatesLogic> logger) {
        _docRepoCass = docRepoCass;
        _docRepoRed = docRepoRed;
        _logger = logger;
    }

    public async Task<bool> VerifyExistsAsync(Guid workspaceId, Guid docId) {
        try {
            if (await _docRepoRed.VerifyDocumentExistsAsync(docId)) {
                return true;
            }

            else if (await _docRepoCass.VerifyExistsAsync(workspaceId, docId)) {
                return true;
            }

            return false;
        }
        catch (Exception) {
            return false;
        }
    }

    public async Task<byte[]> GetDocumentBytes(Guid docId, byte[] stateVector) {
        try {
            List<byte[]> bytes = await _docRepoCass.GetSnapshot(docId, "snapshot1");

            Doc doc = new();
            foreach (var update in bytes) {
                Transaction writeTransaction = doc.WriteTransaction();
                writeTransaction.ApplyV1(update);
                writeTransaction.Commit();
            }

            Transaction readTransaction = doc.ReadTransaction();
            var mergedUpdates = readTransaction.StateDiffV1(stateVector);
            readTransaction.Commit();

            try {
                _docRepoRed.CacheDocumentAsync(docId);
            }
            catch (Exception) {
                _logger.LogError("Error trying to cache content for document {}", docId);
            }

            _logger.LogInformation("Successfully acquired document bytes for document {} from Cassandra, loaded {} updates", docId, bytes.Count());
            return mergedUpdates;

        }
        catch (Exception ec) {
            _logger.LogError("Failed to acquire document bytes for document {} due to {}", docId, ec.Message);
            throw new CantLoadDocumentContentException();
        }
    }

    public async void UpdateDoc(Guid workspaceId, Guid docId, SyncUpdateMessage updateMsg) {
        try {
            await _docRepoRed.SaveUpdateAsync(docId, updateMsg.Update);
            _logger.LogInformation("Update to {}:{} successfully written to message queue", workspaceId, docId);
        }
        catch (Exception ec) {
            _logger.LogError("Update to {}:{} could not be written to message queue due to {}", workspaceId, docId, ec.Message);
            throw new FailedToQueueUpdateException();
        }
    }

}
