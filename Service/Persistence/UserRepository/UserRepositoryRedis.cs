using Models;

namespace Persistence.UserRepository;

public class UserRepositoryRedis {

    /// <summary>
    /// Reads user info from pubsub
    /// </summary>
    public User GetUser() {
        return null;
    }

    /// <summary>
    /// Reads (workspaceId, userId, permissionLevel) from pubsub
    /// </summary>
    public (Guid, Guid, PermissionLevel) ReadWorkspaceUserPermissions() {
        return (Guid.NewGuid(), Guid.NewGuid(), 0);
    }

    /// <summary>
    /// Reads (workspaceId, userId, newPermissionLevel) from pubsub
    /// </summary>
    public void ReadWorkspaceUserNewPermissions() {

    }

}
