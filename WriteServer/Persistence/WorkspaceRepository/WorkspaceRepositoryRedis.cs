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

    /// <summary>
    /// Write workspace info to redis pub sub
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="workspace"></param>
    public void CreateWorkspace(Guid UserID, Workspace workspace) {

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