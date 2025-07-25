using System.Text.Json;
using ApplicationLogic.Dtos;
using Microsoft.Extensions.Logging;
using Models;
using Persistence.DocumentRepository;

namespace ApplicationLogic;

public class UpdateLogic {
    private readonly DocumentRepositoryCassandra _docRepoCass;
    private readonly DocumentRepositoryRedis _docRepoRed;
    private readonly ILogger<UpdateLogic> _logger;

    public UpdateLogic(DocumentRepositoryCassandra docRepoCass, DocumentRepositoryRedis docRepoRed, ILogger<UpdateLogic> logger) {
        _docRepoCass = docRepoCass;
        _docRepoRed = docRepoRed;
        _logger = logger;
    }

    private async Task UpdateCacheForDocument(Guid documentId, byte[] newContent) {
        await _docRepoRed.UpdateCacheForDocument(documentId, newContent);
    }

    public async Task SaveUpdate(string? message) {
        if (message == null)
            return;

        var deserializedMessage = JsonSerializer.Deserialize<UpdateDto>(message);
        if (deserializedMessage == null)
            return;

        byte[] update_bytes = Convert.FromBase64String(deserializedMessage.Update);

        Random random = new();
        long update_id = BitConverter.ToInt64(update_bytes) + random.NextInt64();
        UpdatesBySnapshot update = new() {
            DocumentId = deserializedMessage.DocumentId,
            SnapshotId = "snapshot1",
            UpdateId = update_id,
            PayLoad = update_bytes
        };

        try {
            await UpdateCacheForDocument(update.DocumentId, update_bytes);
            // _logger.LogInformation("Update {}/{}/{} cached", update.DocumentId, update.SnapshotId, update.UpdateId);
        }
        catch (Exception ec) {
            _logger.LogError("Caching update failed due to {}", ec.Message);
        }

        try {
            await _docRepoCass.SaveUpdateAsync(update);
            // _logger.LogInformation("Update {}/{}/{} persisted to cassandra", update.DocumentId, update.SnapshotId, update.UpdateId);
        }
        catch (ArgumentException ec) {
            _logger.LogError("Saving document update {}/{}/{} in cassandra failed due to {}. Retrying with different key", update.DocumentId, update.SnapshotId, update.UpdateId, ec.Message);
            await RetrySaveToCassandra(update);
        }
        catch (Exception ec) {
            _logger.LogError("Saving document update {}/{}/{} in cassandra failed due to {}", update.DocumentId, update.SnapshotId, update.UpdateId, ec.Message);
        }
    }

    public async Task RetrySaveToCassandra(UpdatesBySnapshot update) {
        Random rnd = new();
        update.UpdateId += rnd.NextInt64();

        try {
            await _docRepoCass.SaveUpdateAsync(update);
            // _logger.LogInformation("Update {}/{}/{} persisted to cassandra", update.DocumentId, update.SnapshotId, update.UpdateId);
        }
        catch (ArgumentException ec) {
            _logger.LogError("Saving document update {}/{}/{} in cassandra failed due to {}. Retrying with different key", update.DocumentId, update.SnapshotId, update.UpdateId, ec.Message);
            await RetrySaveToCassandra(update);
        }
        catch (Exception ec) {
            _logger.LogError("Saving document update {}/{}/{} in cassandra failed due to {}", update.DocumentId, update.SnapshotId, update.UpdateId, ec.Message);
        }
    }
}