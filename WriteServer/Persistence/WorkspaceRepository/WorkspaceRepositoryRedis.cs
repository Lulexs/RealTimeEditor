using System.Text.Json;
using Models;
using StackExchange.Redis;

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

    public async Task PublishPermissionChange(Guid workspaceId, string username, PermissionLevel newPermLevel, string performer) {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = "changeuserpermission";
        var message = new {
            WorkspaceId = workspaceId,
            Username = username,
            NewPermission = (int)newPermLevel,
            Performer = performer
        };
        string serializedMessage = JsonSerializer.Serialize(message);

        await subscriber.PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal), serializedMessage);
    }

    public async Task KickUser(Guid workspaceId, string username, string performer) {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = $"kickuser";
        var message = new {
            WorkspaceId = workspaceId,
            Username = username,
            Performer = performer
        };
        string serializedMessage = JsonSerializer.Serialize(message);

        await subscriber.PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal), serializedMessage);
    }

    public async Task ChangeName(Guid workspaceID, string newName) {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = $"changeworkspacename";
        var message = new {
            WorkspaceId = workspaceID,
            NewName = newName
        };
        string serializedMessage = JsonSerializer.Serialize(message);

        await subscriber.PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal), serializedMessage);
    }
}