using Models;

namespace Persistence.UserRepository;

public class UserRepositoryCassandra
{

    /// <summary>
    /// Writes user info to cassandra 
    /// </summary>
    public async Task SaveUser(RegisterDto user)
    {
        var session = CassandraSessionManager.GetSession();
        
        var query = "INSERT INTO users_by_username (username, region, userid, password, email, avatar) VALUES (?, ?, ?, ?, ?, ?)";

        var preparedStatement = await session.PrepareAsync(query);

        var boundStatement = preparedStatement.Bind(
            user.Username,
            user.Region,
            Guid.NewGuid(),
            user.Password,
            user.Email,
            user.Avatar
        )

        var result = await session.ExecuteAsync(boundStatement);

        if (result.Any())
            return true;
        return false;


    }

    /// <summary>
    /// Adds user to workspace and sets specified permission level to cassandra
    /// </summary>
    public void AddUserToWorkspace()
    {

    }

    /// <summary>
    /// Change user's permission level in cassandra
    /// </summary>
    public void ChangeUserPermissionLevel() {

    }

}
