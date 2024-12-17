using Models;

namespace Persistence.WorkspaceRepository;

public class WorkspaceRepositoryRedis {
    /// <summary>
    /// Get workspace names from cache
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public List<Workspace> GetUsersWorkspaces(Guid UserID) {
        return [];
    }

    /// <summary>
    /// Write workspace info to redis pub sub
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="workspace"></param>
    public void CreateWorkspace(Guid UserID, Workspace workspace) {

    }

    /// <summary>
    /// Write new workspace name to redis pub sub
    /// </summary>
    /// <param name="workspaceID"></param>
    /// <param name="newName"></param>
    public void ChangeName(Guid workspaceID, string newName) {

    }
}