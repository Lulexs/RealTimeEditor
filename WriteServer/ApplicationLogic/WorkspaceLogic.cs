using ApplicationLogic.Dtos;
using ApplicationLogic.Exceptions;
using Persistence.WorkspaceRepository;
using Models;
using Persistence.DocumentRepository;

namespace ApplicationLogic;

public class WorkspaceLogic {
    private readonly WorkspaceRepositoryCassandra _wsRepoCass;
    private readonly WorkspaceRepositoryRedis _wsRepoRed;
    private readonly DocumentRepositoryCassandra _docRepoCass;

    public WorkspaceLogic(WorkspaceRepositoryCassandra wsRepoCass, WorkspaceRepositoryRedis wsRepoRed, DocumentRepositoryCassandra docRepoCass) {
        _wsRepoCass = wsRepoCass;
        _wsRepoRed = wsRepoRed;
        _docRepoCass = docRepoCass;
    }

    public async Task<Workspace> CreateWorkspace(WorkspaceDto dto) {
        var workspace = new Workspace {
            Username = dto.OwnerName,
            WorkspaceId = Guid.NewGuid(),
            WorkspaceName = dto.Name,
            OwnerUsername = dto.OwnerName,
            Permission = PermissionLevel.OWNER,
            CreatedAt = DateTime.UtcNow,
        };

        await _wsRepoCass.CreateWorkspaceAsync(workspace);
        return workspace;

    }

    public async Task ChangeWorkspaceName(ChangeWorkspaceNameDto dto) {
        var documentExists = await _wsRepoCass.VerifyExistsAsync(dto.WorkspaceId);
        if (!documentExists) {
            throw new WorkspaceNotFoundException($"Document {dto.WorkspaceId} not found");
        }
        await _wsRepoRed.ChangeName(dto.WorkspaceId, dto.NewName);
    }

    public async Task KickUserFromWorkspace(Guid workspaceId, string username, string performer) {

        var workspaceExists = await _wsRepoCass.VerifyExistsAsync(workspaceId);
        if (!workspaceExists) {
            throw new WorkspaceNotFoundException($"Workspace {workspaceId} not found");
        }

        await _wsRepoRed.KickUser(workspaceId, username, performer);
    }

    public async Task ChangeUserPermission(Guid workspaceId, string username, PermissionLevel newPermLevel, string performer) {
        var workspaceExists = await _wsRepoCass.VerifyExistsAsync(workspaceId);
        if (!workspaceExists) {
            throw new WorkspaceNotFoundException($"Workspace {workspaceId} not found");
        }
        await _wsRepoRed.PublishPermissionChange(workspaceId, username, newPermLevel, performer);
    }

    public async Task<List<Workspace>> GetUserWorkspaces(string username) {
        var workspaces = await _wsRepoCass.GetUserWorkspaces(username);
        return workspaces;
    }

    public async Task<List<string>> UsersInWorkspace(Guid workspaceId) {
        return await _wsRepoCass.UsersInWorkspace(workspaceId);
    }

    public async Task<Workspace?> GetWorkspaceByUserAndId(string username, Guid workspaceId) {
        return await _wsRepoCass.GetWorkspaceByUserAndId(username, workspaceId);
    }

    public async Task<Workspace> AddUserToWorkspace(JoinWorkspaceDto dto) {

        var decomposed = dto.JoinCode.Split("\\");
        var workspaceId = Guid.Parse(decomposed[0]);
        PermissionLevel permissionLevel = (PermissionLevel)int.Parse(decomposed[1]);

        if ((await _wsRepoCass.UserInWorkspaceCheck(dto.Username, workspaceId)).Count() != 0) {
            throw new Exception("You are already in this workspace");
        }

        var usersInWorkspace = await UsersInWorkspace(workspaceId);

        if (usersInWorkspace.Count == 0) {
            throw new WorkspaceNotFoundException("Workspace doesn't exist");
        }

        var anyUserUsername = usersInWorkspace.First();

        var workspaceInfo = await GetWorkspaceByUserAndId(anyUserUsername, workspaceId);

        await _wsRepoCass.AddUserToWorkspace(workspaceInfo!, dto.Username);
        return new Workspace {
            Username = dto.Username,
            WorkspaceId = workspaceInfo!.WorkspaceId,
            WorkspaceName = workspaceInfo.WorkspaceName,
            OwnerUsername = workspaceInfo.OwnerUsername,
            Permission = permissionLevel,
            CreatedAt = workspaceInfo.CreatedAt
        };
    }

    public async Task<List<UserInWorkspaceDto>> GetUsersInWorkspace(Guid workspaceId) {
        var resultSet = await _wsRepoCass.GetUsersInWorkspace(workspaceId);
        var usersInWorkspace = new List<UserInWorkspaceDto>();
        foreach (var row in resultSet) {
            usersInWorkspace.Add(new UserInWorkspaceDto {
                WorkspaceId = workspaceId,
                Username = row.GetValue<string>("username"),
                Permission = (PermissionLevel)row.GetValue<int>("permissionlevel")
            });
        }

        return usersInWorkspace;
    }

    public async Task DeleteWorkspace(Guid workspaceId, string username) {
        var workspace = await _wsRepoCass.GetWorkspaceByUserAndId(username, workspaceId);
        if (workspace == null) {
            throw new WorkspaceNotFoundException($"Workspace {workspaceId} not found");
        }

        var creator = workspace.OwnerUsername;
        if (creator != username) {
            throw new UnauthorizedAccessException("Only the workspace owner can delete it.");
        }

        var usersInWorkspace = await _wsRepoCass.UsersInWorkspace(workspaceId);

        var documents = await _docRepoCass.GetDocumentIdsInWorkspace(workspaceId);
        foreach (var documentId in documents) {
            await _docRepoCass.DeleteDocumentUpdates(documentId);
        }

        await _docRepoCass.DeleteDocuments(workspaceId);

        foreach (var workspaceUser in usersInWorkspace) {
            await _wsRepoCass.DeleteWorkspaceEntries(workspaceId, workspaceUser);
        }

        await _wsRepoCass.DeleteUserEntries(workspaceId);

    }

}