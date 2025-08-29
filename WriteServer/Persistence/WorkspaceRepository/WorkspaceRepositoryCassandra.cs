using Models;

namespace Persistence.WorkspaceRepository;

public class WorkspaceRepositoryCassandra {

    public async Task<bool> VerifyExistsAsync(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync("SELECT workspaceid FROM users_by_workspace WHERE workspaceid = ?");
        var boundStatement = statement.Bind(workspaceId);
        var result = (await session.ExecuteAsync(boundStatement)).ToList();

        return result.Count != 0;
    }

    public async Task CreateWorkspaceAsync(Workspace workspace) {
        var session = CassandraSessionManager.GetSession();

        //`workspaces_by_user`
        var workspacesByUserStatement = await session.PrepareAsync(
            "INSERT INTO workspaces_by_user (username, workspaceid, workspacename, createrusername, permissionlevel, createdat) " +
            "VALUES (?, ?, ?, ?, ?, ?)"
        );
        var workspacesByUserBound = workspacesByUserStatement.Bind(
            workspace.Username, workspace.WorkspaceId, workspace.WorkspaceName, workspace.OwnerUsername, (int)workspace.Permission, workspace.CreatedAt
        );
        await session.ExecuteAsync(workspacesByUserBound);

        //`users_by_workspace`
        var usersByWorkspaceStatement = await session.PrepareAsync(
            "INSERT INTO users_by_workspace (workspaceid, username, permissionlevel) " +
            "VALUES (?, ?, ?)"
        );
        var usersByWorkspaceBound = usersByWorkspaceStatement.Bind(
            workspace.WorkspaceId, workspace.Username, (int)PermissionLevel.OWNER
        );
        await session.ExecuteAsync(usersByWorkspaceBound);
    }

    public async Task<List<Workspace>> GetUserWorkspaces(string username) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync(
            "SELECT workspaceid, workspacename, createrusername, permissionlevel, createdat " +
            "FROM workspaces_by_user WHERE username = ?"
        );
        var boundStatement = statement.Bind(username);

        var resultSet = await session.ExecuteAsync(boundStatement);

        var workspaces = new List<Workspace>();
        foreach (var row in resultSet) {
            workspaces.Add(new Workspace {
                Username = username,
                WorkspaceId = row.GetValue<Guid>("workspaceid"),
                WorkspaceName = row.GetValue<string>("workspacename"),
                OwnerUsername = row.GetValue<string>("createrusername"),
                Permission = (PermissionLevel)row.GetValue<int>("permissionlevel"),
                CreatedAt = row.GetValue<DateTime>("createdat")
            });
        }

        return workspaces;
    }

    public async Task<List<string>> UserInWorkspaceCheck(string username, Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();
        var statement = await session.PrepareAsync(
            "SELECT username FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
        );
        var boundStatement = statement.Bind(workspaceId, username);
        var result = (await session.ExecuteAsync(boundStatement)).ToList();

        return result.Select(row => row.GetValue<string>("username")).ToList();

    }

    public async Task<List<string>> UsersInWorkspace(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();
        var anyUserInWorkspaceStatement = await session.PrepareAsync(
                    "SELECT username FROM users_by_workspace WHERE workspaceid = ?"
                );
        var anyUserInWorkspaceStatementBound = anyUserInWorkspaceStatement.Bind(workspaceId);
        var res = (await session.ExecuteAsync(anyUserInWorkspaceStatementBound)).ToList();

        return res.Select(row => row.GetValue<string>("username")).ToList();
    }

    public async Task<Workspace?> GetWorkspaceByUserAndId(string username, Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();
        var workspaceInfoStatement = await session.PrepareAsync("SELECT workspacename, createrusername, permissionlevel, createdat FROM workspaces_by_user WHERE username = ? AND workspaceid = ?");
        var workspaceInfoStatementBound = workspaceInfoStatement.Bind(username, workspaceId);
        var workspaceInfoRow = (await session.ExecuteAsync(workspaceInfoStatementBound)).FirstOrDefault();
        return workspaceInfoRow != null ? new Workspace {
            WorkspaceId = workspaceId,
            WorkspaceName = workspaceInfoRow.GetValue<string>("workspacename"),
            OwnerUsername = workspaceInfoRow.GetValue<string>("createrusername"),
            CreatedAt = workspaceInfoRow.GetValue<DateTime>("createdat"),
            Username = username,
            Permission = (PermissionLevel)workspaceInfoRow.GetValue<int>("permissionlevel"),
        } : null;
    }

    public async Task AddUserToWorkspace(Workspace workspace, string username) {
        var session = CassandraSessionManager.GetSession();
        var workspacesByUserStatement = await session.PrepareAsync(
                        "INSERT INTO workspaces_by_user (username, workspaceid, workspacename, createrusername, permissionlevel, createdat) " +
                        "VALUES (?, ?, ?, ?, ?, ?)"
                    );
        var workspacesByUserBound = workspacesByUserStatement.Bind(
            username, workspace.WorkspaceId, workspace.WorkspaceName, workspace.OwnerUsername, (int)workspace.Permission, workspace.CreatedAt
        );
        await session.ExecuteAsync(workspacesByUserBound);

        var usersByWorkspaceStatement = await session.PrepareAsync(
            "INSERT INTO users_by_workspace (workspaceid, username, permissionlevel) " +
            "VALUES (?, ?, ?)"
        );
        var usersByWorkspaceBound = usersByWorkspaceStatement.Bind(
            workspace.WorkspaceId, username, (int)workspace.Permission
        );
        await session.ExecuteAsync(usersByWorkspaceBound);
    }

    public async Task<Cassandra.RowSet> GetUsersInWorkspace(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();
        var statement = await session.PrepareAsync(
            "SELECT username, permissionlevel FROM users_by_workspace WHERE workspaceid = ?"
        );
        var boundStatement = statement.Bind(workspaceId);
        return await session.ExecuteAsync(boundStatement);
    }

    public async Task DeleteWorkspaceEntries(Guid workspaceId, string username) {
        var session = CassandraSessionManager.GetSession();
        var deleteWorkspaceByUserStatement = await session.PrepareAsync(
                    "DELETE FROM workspaces_by_user WHERE username = ? AND workspaceid = ?"
                );
        var deleteWorkspaceByUserBound = deleteWorkspaceByUserStatement.Bind(username, workspaceId);
        await session.ExecuteAsync(deleteWorkspaceByUserBound);
    }

    public async Task DeleteUserEntries(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();
        var deleteUsersStatement = await session.PrepareAsync(
            "DELETE FROM users_by_workspace WHERE workspaceid = ?"
        );
        var deleteUsersBound = deleteUsersStatement.Bind(workspaceId);
        await session.ExecuteAsync(deleteUsersBound);
    }

}