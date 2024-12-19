using System.Text.Json;
using ApplicationLogic.Dtos;
using Cassandra;
using Models;
using Persistence;
using Persistence.DocumentRepository;

namespace ApplicationLogic;

public class UpdateLogic {
    private readonly DocumentRepositoryCassandra _docRepoCass;

    public UpdateLogic(DocumentRepositoryCassandra docRepoCass) {
        _docRepoCass = docRepoCass;
    }

    public async Task SaveUpdate(string? message) {
        if (message == null)
            return;

        var deserializedMessage = JsonSerializer.Deserialize<UpdateDto>(message);
        if (deserializedMessage == null)
            return;

        byte[] update_bytes = Convert.FromBase64String(deserializedMessage.Update);
        long update_id = BitConverter.ToInt64(update_bytes);
        UpdatesBySnapshot update = new() {
            DocumentId = deserializedMessage.DocumentId,
            SnapshotId = "snapshot1",
            UpdateId = update_id,
            PayLoad = update_bytes
        };

        await _docRepoCass.SaveUpdateAsync(update);
    }
}