using ApplicationLogic.Dtos;
using Persistence.WorkspaceRepository;
using System.Text.Json;

namespace ApplicationLogic;

public class WorkspaceLogic {
    private readonly WorkspaceRepositoryCassandra _wsRepoCass;

    public WorkspaceLogic(WorkspaceRepositoryCassandra wsRepoCass) {
        _wsRepoCass = wsRepoCass;
    }

    public async Task ChangeWorkspaceName(string? message) {
        if (message == null) {
            return;
        }

        try {
            var deserializedMessage = JsonSerializer.Deserialize<ChangeWorkspaceNameDto>(message);
            if (deserializedMessage == null) {
                Console.WriteLine("Recieved corrupted message for changing workspace name");
                return;
            }
            var usernames = await _wsRepoCass.GetUsernamesInWorkspace(deserializedMessage.WorkspaceId);

            foreach (var username in usernames) {
                await _wsRepoCass.ChangeName(deserializedMessage.WorkspaceId, username, deserializedMessage.NewName);
            }

        }
        catch (Exception ec) {
            Console.WriteLine($"Error changing workspace name {ec.Message}");
        }

    }
}