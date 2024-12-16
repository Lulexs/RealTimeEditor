using Models;

namespace Persistence.WorkspaceRepository;

public class WorkspaceRepositoryRedis
{
    /// <summary>
    /// Get workspace names from cache
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public List<Workspace> GetUsersWorkspaces(GUID UserID)
    {

    }

    /// <summary>
    /// Write workspace info to redis pub sub
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="workspace"></param>
    public void CreateWorkspace(GUID UserID, Workspace workspace)
    {

    }

    /// <summary>
    /// Write new workspace name to redis pub sub
    /// </summary>
    /// <param name="workspaceID"></param>
    /// <param name="newName"></param>
    public void ChangeName(GUID workspaceID, string newName) {

    }
}