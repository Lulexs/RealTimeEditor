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
}