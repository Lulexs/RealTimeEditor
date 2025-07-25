using Models;
using System.Text.Json;
using StackExchange.Redis;

namespace Persistence.UserRepository;

public class UserRepositoryRedis {

    public async Task SaveUser(User user) {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = "register";

        var message = new {
            user.UserId,
            user.Username,
            Password = user.HashedPassword,
            user.Region,
            user.Avatar,
            user.Email
        };

        string serializedMessage = JsonSerializer.Serialize(message);

        await subscriber.PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal), serializedMessage);
    }

    /// <summary>
    /// Get all users that are added to workspace from cache
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public List<User> GetUsersInWorkspace(Guid workspaceId) {
        return [];
    }

    /// <summary>
    /// Adds user to workspace and sets specified permission level
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public void AddUserToWorkspace(Guid workspaceId, Guid userId, PermissionLevel permission) {

    }

    /// <summary>
    /// Change user's permission level for workspace
    /// This function does not check if user has privilleges to change other users permission level
    /// That is handled in application logic
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="permission"></param>
    public void ChangeUserPermissionLevel(Guid userId, Guid workspaceId, PermissionLevel permission) {

    }

}
