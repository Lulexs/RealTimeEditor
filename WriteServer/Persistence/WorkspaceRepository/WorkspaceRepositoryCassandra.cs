using Models;



namespace Persistence.WorkspaceRepository;

public class WorkspaceRepositoryCassandra {
    /// <summary>
    /// Get workspace names
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public List<Workspace> GetUsersWorkspaces(Guid UserID) {
        return [];
    }

    public async Task<bool> VerifyExistsAsync(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync("SELECT workspaceid FROM users_by_workspace WHERE workspaceid = ?");
        var boundStatement = statement.Bind(workspaceId);
        var result = await session.ExecuteAsync(boundStatement);

        if (result.Any())
            return true;
        return false;
    }

    public async Task<PermissionLevel?> GetUserPermissionAsync(Guid workspaceId, string username)
    {
        var session = CassandraSessionManager.GetSession();
        var statement = await session.PrepareAsync(
            "SELECT permissionlevel FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
        );
        var boundStatement = statement.Bind(workspaceId, username);
        var resultSet = await session.ExecuteAsync(boundStatement);

        if (!resultSet.Any())
        {
            return null;
        }

        var row = resultSet.First();
        return (PermissionLevel)row.GetValue<int>("permissionlevel");
    }

    public async Task<bool> VerifyUserExistsInWorkspaceAsync(Guid workspaceId, string username)
    {
        var session = CassandraSessionManager.GetSession();
        var statement = await session.PrepareAsync(
            "SELECT username FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
        );
        var boundStatement = statement.Bind(workspaceId, username);
        var result = await session.ExecuteAsync(boundStatement);
        return result.Any();
    }

    public async Task RemoveUserFromWorkspaceAsync(Guid workspaceId, string username)
    {
        var session = CassandraSessionManager.GetSession();
        var statement = await session.PrepareAsync(
            "DELETE FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
        );
        var boundStatement = statement.Bind(workspaceId, username);
        await session.ExecuteAsync(boundStatement);
    }


}