using Models;

namespace Persistence.WorkspaceRepository;

public class WorkspaceRepositoryCassandra {
    /// <summary>
    /// Write workspace info to cassandra
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="workspace"></param>
    public void CreateWorkspace() {

    }

    public async Task<List<string>> GetUsernamesInWorkspace(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync("SELECT username FROM users_by_workspace WHERE workspaceid = ?");
        var boundStatement = statement.Bind(workspaceId);

        var resultSet = await session.ExecuteAsync(boundStatement);

        var usernames = new List<string>();
        foreach (var row in resultSet) {
            usernames.Add(row.GetValue<string>("username"));
        }

        return usernames;
    }

    public async Task ChangeName(Guid workspaceId, string username, string newName) {
        var session = CassandraSessionManager.GetSession();

        var updateWorkspacesByUserStatement = await session.PrepareAsync(
            "UPDATE workspaces_by_user SET workspacename = ? WHERE username = ? and workspaceid = ?"
        );
        var boundUpdateStatement = updateWorkspacesByUserStatement.Bind(newName, username, workspaceId);
        await session.ExecuteAsync(boundUpdateStatement);
    }

    public async Task<bool> VerifyExistsAsync(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync("SELECT workspaceid FROM users_by_workspace WHERE workspaceid = ?");
        var boundStatement = statement.Bind(workspaceId);
        var result = (await session.ExecuteAsync(boundStatement)).ToList();

        if (result.Count != 0)
            return true;
        return false;
    }

    public async Task<PermissionLevel?> GetUserPermissionAsync(Guid workspaceId, string username) {
        var session = CassandraSessionManager.GetSession();
        var statement = await session.PrepareAsync(
            "SELECT permissionlevel FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
        );
        var boundStatement = statement.Bind(workspaceId, username);
        var resultSet = (await session.ExecuteAsync(boundStatement)).ToList();

        if (resultSet.Count == 0) {
            return null;
        }

        var row = resultSet.First();
        return (PermissionLevel)row.GetValue<int>("permissionlevel");
    }

    public async Task<bool> VerifyUserExistsInWorkspaceAsync(Guid workspaceId, string username) {
        var session = CassandraSessionManager.GetSession();
        var statement = await session.PrepareAsync(
            "SELECT username FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
        );
        var boundStatement = statement.Bind(workspaceId, username);
        var result = (await session.ExecuteAsync(boundStatement)).ToList();
        return result.Count != 0;
    }

    public async Task RemoveUserFromWorkspaceAsync(Guid workspaceId, string username) {
        var session = CassandraSessionManager.GetSession();
        var statement = await session.PrepareAsync(
            "DELETE FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
        );
        var boundStatement = statement.Bind(workspaceId, username);
        await session.ExecuteAsync(boundStatement);

        var statement1 = await session.PrepareAsync(
            "DELETE FROM workspaces_by_user WHERE username = ? and workspaceid = ?"
        );
        var boundStatement1 = statement1.Bind(username, workspaceId);
        await session.ExecuteAsync(boundStatement1);
    }
    public async Task UpdateUserPermissionAsync(Guid workspaceId, string username, int newPermissionLevel)
    {
        var session = CassandraSessionManager.GetSession();
        var updateUsersByWorkspaceStatement = await session.PrepareAsync(
            "UPDATE users_by_workspace SET permissionlevel = ? WHERE workspaceid = ? AND username = ?"
        );
        var updateUsersByWorkspaceBound = updateUsersByWorkspaceStatement.Bind(newPermissionLevel, workspaceId, username);
        await session.ExecuteAsync(updateUsersByWorkspaceBound);

        var updateWorkspacesByUserStatement = await session.PrepareAsync(
            "UPDATE workspaces_by_user SET permissionlevel = ? WHERE username = ? AND workspaceid = ?"
        );
        var updateWorkspacesByUserBound = updateWorkspacesByUserStatement.Bind(newPermissionLevel, username, workspaceId);
        await session.ExecuteAsync(updateWorkspacesByUserBound);
    }
}