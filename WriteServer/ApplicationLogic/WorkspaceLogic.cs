using ApplicationLogic.Dtos;
using ApplicationLogic.Exceptions;
using Persistence.WorkspaceRepository;
using Models;

namespace ApplicationLogic;

public class WorkspaceLogic {
    private readonly WorkspaceRepositoryCassandra _wsRepoCass;
    private readonly WorkspaceRepositoryRedis _wsRepoRed;

    public WorkspaceLogic(WorkspaceRepositoryCassandra wsRepoCass, WorkspaceRepositoryRedis wsRepoRed) {
        _wsRepoCass = wsRepoCass;
        _wsRepoRed = wsRepoRed;
    }

    public async Task ChangeWorkspaceName(ChangeWorkspaceNameDto dto) {
        var documentExists = await _wsRepoCass.VerifyExistsAsync(dto.WorkspaceId);
        if (!documentExists) {
            throw new WorkspaceNotFoundException($"Document {dto.WorkspaceId} not found");
        }
        await _wsRepoRed.ChangeName(dto.WorkspaceId, dto.NewName);
    }

    public async Task KickUserFromWorkspace(Guid workspaceId, string username, string performer)
    {

        var workspaceExists = await _wsRepoCass.VerifyExistsAsync(workspaceId);
        if (!workspaceExists)
        {
            throw new WorkspaceNotFoundException($"Workspace {workspaceId} not found");
        }

        
        var performerPermission = await _wsRepoCass.GetUserPermissionAsync(workspaceId, performer);
        if (performerPermission != PermissionLevel.ADMIN && performerPermission != PermissionLevel.OWNER)
        {
            throw new UnauthorizedAccessException("Only admins or owners can kick users from the workspace.");
        }

        
        var userExists = await _wsRepoCass.VerifyUserExistsInWorkspaceAsync(workspaceId, username);
        if (!userExists)
        {
            throw new WorkspaceNotFoundException($"User {username} not found in workspace {workspaceId}");
        }

        
        await _wsRepoCass.RemoveUserFromWorkspaceAsync(workspaceId, username);

        
    }


}