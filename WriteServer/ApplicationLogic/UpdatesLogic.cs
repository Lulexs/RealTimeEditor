
using ApplicationLogic.Exceptions;
using Microsoft.Extensions.Logging;
using Persistence.DocumentRepository;
using StackExchange.Redis;
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
        }
        catch (Exception e) {
            _logger.LogError("Error trying to verify document's existance using redis: {}", e.Message);
        }

        try {
            if (await _docRepoCass.VerifyExistsAsync(workspaceId, docId)) {
                return true;
            }
        }
        catch (Exception e) {
            _logger.LogError("Error trying to verify document's existance using cassandra: {}", e.Message);
        }

        return false;
    }

    public async Task<byte[]> GetDocumentBytes(Guid docId, string snapshotId, byte[] stateVector) {
        try {
            RedisValue[] docContent = await _docRepoRed.LoadCachedDocumentAsync(docId, snapshotId);
            if (docContent.Length != 0) {
                _logger.LogInformation("Successfully acquired document bytes for document {}/{} from Redis", docId, snapshotId);

                Doc doc = new();
                foreach (var update in docContent) {
                    Transaction writeTransaction = doc.WriteTransaction();
                    writeTransaction.ApplyV1(update!);
                    writeTransaction.Commit();
                }

                Transaction readTransaction = doc.ReadTransaction();
                var mergedUpdates = readTransaction.StateDiffV1(stateVector);
                readTransaction.Commit();

                return mergedUpdates;
            }
        }
        catch (Exception ec) {
            _logger.LogError("Failed to acquired document content from Redis due to {}", ec.Message);
        }

        try {
            List<byte[]> bytes = await _docRepoCass.GetSnapshot(docId, snapshotId);

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
                await _docRepoRed.CacheDocumentAsync(docId, snapshotId, mergedUpdates);
            }
            catch (Exception) {
                _logger.LogError("Error trying to cache content for document {}/{}", docId, snapshotId);
            }

            _logger.LogInformation("Successfully acquired document bytes for document {}/{} from Cassandra, loaded {} updates", docId, snapshotId, bytes.Count);
            return mergedUpdates;

        }
        catch (Exception ec) {
            _logger.LogError("Failed to acquire document bytes for document {}/{} due to {}", docId, snapshotId, ec.Message);
            throw new CantLoadDocumentContentException();
        }
    }

    public async void UpdateDoc(Guid workspaceId, Guid docId, SyncUpdateMessage updateMsg) {
        try {
            await _docRepoRed.SaveUpdateAsync(docId, updateMsg.Update);
            _logger.Log(LogLevel.None, "Update to {}:{} successfully written to message queue", workspaceId, docId);
        }
        catch (Exception ec) {
            _logger.LogError("Update to {}:{} could not be written to message queue due to {}", workspaceId, docId, ec.Message);
            throw new FailedToQueueUpdateException();
        }
    }

}
