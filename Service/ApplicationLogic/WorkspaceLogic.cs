using ApplicationLogic.Dtos;
using Models;
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

    public async Task KickUserFromWorkspace(string? message) {
        if (message == null) {
            return;
        }
        try {
            var deserializedMessage = JsonSerializer.Deserialize<KickUserDto>(message);

            if (deserializedMessage == null) {
                return;
            }

            var workspaceExists = await _wsRepoCass.VerifyExistsAsync(deserializedMessage.WorkspaceId);
            if (!workspaceExists) {
                Console.WriteLine($"Workspace {deserializedMessage.WorkspaceId} not found");
                return;
            }

            var performerPermission = await _wsRepoCass.GetUserPermissionAsync(deserializedMessage.WorkspaceId, deserializedMessage.Performer);
            if (performerPermission != PermissionLevel.ADMIN && performerPermission != PermissionLevel.OWNER) {
                Console.WriteLine("Only admins or owners can kick users from the workspace.");
                return;
            }

            var userExists = await _wsRepoCass.VerifyUserExistsInWorkspaceAsync(deserializedMessage.WorkspaceId, deserializedMessage.Username);
            if (!userExists) {
                Console.WriteLine($"User {deserializedMessage.Username} not found in workspace {deserializedMessage.WorkspaceId}");
                return;
            }

            await _wsRepoCass.RemoveUserFromWorkspaceAsync(deserializedMessage.WorkspaceId, deserializedMessage.Username);
        }
        catch (Exception ex) {
            Console.WriteLine("Error trying to deserialize message for kicking user. " + ex.Message);
        }
    }

    public async Task ApplyUserPermissionChange(string? message)
    {
        if (message == null)
        {
            return;
        }
        try
        {
            var deserializedMessage = JsonSerializer.Deserialize<ChangePermissionDto>(message);

            if (deserializedMessage == null)
            {
                return;
            }

            
            var workspaceExists = await _wsRepoCass.VerifyExistsAsync(deserializedMessage.WorkspaceId);
            if (!workspaceExists)
            {
                Console.WriteLine($"Workspace {deserializedMessage.WorkspaceId} not found");
                return;
            }

            
            var performerPermission = await _wsRepoCass.GetUserPermissionAsync(deserializedMessage.WorkspaceId, deserializedMessage.Performer);
            if (performerPermission != PermissionLevel.ADMIN && performerPermission != PermissionLevel.OWNER)
            {
                Console.WriteLine("Only admins or owners can change user permissions in the workspace.");
                return;
            }

            
            var userExists = await _wsRepoCass.VerifyUserExistsInWorkspaceAsync(deserializedMessage.WorkspaceId, deserializedMessage.Username);
            if (!userExists)
            {
                Console.WriteLine($"User {deserializedMessage.Username} not found in workspace {deserializedMessage.WorkspaceId}");
                return;
            }

            // Ažuriranje permisije korisnika
            await _wsRepoCass.UpdateUserPermissionAsync(deserializedMessage.WorkspaceId, deserializedMessage.Username, deserializedMessage.NewPermission);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error trying to deserialize message for changing user permission. " + ex.Message);
        }
    }



}