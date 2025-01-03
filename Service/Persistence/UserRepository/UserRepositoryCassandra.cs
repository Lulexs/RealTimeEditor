using Models;

namespace Persistence.UserRepository;

public class UserRepositoryCassandra {

    public async Task<bool> SaveUser(User user) {
        var session = CassandraSessionManager.GetSession();

        var query = "INSERT INTO users_by_username (username, region, userid, password, email, avatar) VALUES (?, ?, ?, ?, ?, ?)";

        var preparedStatement = await session.PrepareAsync(query);

        var boundStatement = preparedStatement.Bind(
            user.Username,
            user.Region,
            user.UserId,
            user.HashedPassword,
            user.Email,
            user.Avatar
        );

        var result = await session.ExecuteAsync(boundStatement);

        if (result.Any())
            return true;

        return false;
    }

    /// <summary>
    /// Adds user to workspace and sets specified permission level to cassandra
    /// </summary>
    public void AddUserToWorkspace() {

    }

    /// <summary>
    /// Change user's permission level in cassandra
    /// </summary>
    public void ChangeUserPermissionLevel() {

    }

}
