using Models;

namespace Persistence.WorkspaceRepository;

public class WorkspaceRepositoryRedis {
    /// <summary>
    /// Reads user id and workspace from pubsub
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="workspace"></param>
    public (Guid, Workspace) GetUserIdeWorkspace() {
        return (Guid.NewGuid(), null);
    }

    /// <summary>
    /// Write new workspace name to redis pub sub
    /// </summary>
    /// <param name="workspaceID"></param>
    /// <param name="newName"></param>
    public (Guid, string) GetWorkspaceChangeName() {
        return (Guid.NewGuid(), null);
    }
}