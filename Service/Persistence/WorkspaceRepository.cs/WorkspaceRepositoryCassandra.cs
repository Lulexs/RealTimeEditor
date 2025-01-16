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
}