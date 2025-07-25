using Models;

namespace Persistence.UserRepository;

public class UserRepositoryCassandra {

    public async Task<User?> GetUserByUsernameAsync(string username) {
        var session = CassandraSessionManager.GetSession();
        var query = "SELECT region, userid, username, email, password, avatar FROM users_by_username WHERE username = ?";
        var prepared = await session.PrepareAsync(query);
        var statement = prepared.Bind(username);

        var rowSet = await session.ExecuteAsync(statement);
        var row = rowSet.FirstOrDefault();

        if (row == null) return null;

        return new User {
            Region = row.GetValue<string>("region"),
            UserId = row.GetValue<Guid>("userid"),
            Username = row.GetValue<string>("username"),
            Email = row.GetValue<string>("email"),
            HashedPassword = row.GetValue<string>("password"), // PAZI U BAZI SE PAMTI HASHOVANA LOZINKA
            Avatar = row.GetValue<string>("avatar")
        };
    }

    /// <summary>
    /// Get all users that are added to workspace
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public List<User> GetUsersInWorkspace(Guid WorkspaceId) {
        return [];
    }
}