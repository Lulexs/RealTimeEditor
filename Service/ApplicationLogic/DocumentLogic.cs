using ApplicationLogic.Dtos;
using Persistence.DocumentRepository;

using System.Text.Json;

namespace ApplicationLogic;

public class DocumentLogic {
    private readonly DocumentRepositoryCassandra _docRepoCass;

    public DocumentLogic(DocumentRepositoryCassandra docRepoCass) {
        _docRepoCass = docRepoCass;
    }

    public async Task ChangeDocumentName(string? message) {
        if (message == null) {
            return;
        }

        try {
            var deserializedMessage = JsonSerializer.Deserialize<ChangeDocumentNameDto>(message);
            if (deserializedMessage == null) {
                Console.WriteLine("Recieved corrupted message for changing document name");
                return;
            }
            await _docRepoCass.ChangeDocumentName(deserializedMessage.WorkspaceId, deserializedMessage.DocumentId, deserializedMessage.NewName);
        }
        catch (Exception ec) {
            Console.WriteLine($"Error changing document name {ec.Message}");
        }

    }
}