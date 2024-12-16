using Models;

namespace Persistence.UserRepository;

public class UserRepositoryCassandra
{

    /// <summary>
    /// Reads user info from pubsub
    /// </summary>
    public User GetUser()
    {

    }

    /// <summary>
    /// Reads (workspaceId, userId, permissionLevel) from pubsub
    /// </summary>
    public (GUID, GUID, PermissionLevel) ReadWorkspaceUserPermissions()
    {

    }

    /// <summary>
    /// Reads (workspaceId, userId, newPermissionLevel) from pubsub
    /// </summary>
    public void ReadWorkspaceUserNewPermissions() {

    }

}
