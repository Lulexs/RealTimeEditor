using Models;

namespace Persistence.UserRepository;

public class UserRepositoryCassandra
{

    /// <summary>
    /// Get user info
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public User GetUserByUsername(string username)
    {
        
    }

    /// <summary>
    /// Get all users that are added to workspace
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public List<User> GetUsersInWorkspace(GUID WorkspaceId)
    {
        
    }
}