using System.Text.Json;
using Models;
using StackExchange.Redis;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryRedis {

    public async Task<bool> VerifyDocumentExistsAsync(Guid documentId) {
        var db = RedisSessionManager.GetDatabase();

        bool exists = await db.KeyExistsAsync($"doc:{documentId}:content");
        return exists;
    }

    public async Task<RedisValue[]> LoadCachedDocumentAsync(Guid docId, string snapshotId) {
        var db = RedisSessionManager.GetDatabase();

        RedisValue[] content = await db.SetMembersAsync($"doc:{docId}-{snapshotId}:content");

        return content;
    }

    public async Task CacheDocumentAsync(Guid docId, string snapshotId, byte[] content) {
        IDatabase database = RedisSessionManager.GetDatabase();

        await database.SetAddAsync($"doc:{docId}-{snapshotId}:content", content);
    }

    public async Task SaveUpdateAsync(Guid documentId, byte[] update) {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = $"realtimeupdate-{documentId}";
        var message = new {
            DocumentId = documentId,
            Update = Convert.ToBase64String(update)
        };
        string serializedMessage = JsonSerializer.Serialize(message);

        await subscriber.PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal), serializedMessage);
    }

    public async Task ChangeDocumentName(Guid workspaceId, Guid documentId, string newName) {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = $"changedocname";
        var message = new {
            WorkspaceId = workspaceId,
            DocumentId = documentId,
            NewName = newName
        };
        string serializedMessage = JsonSerializer.Serialize(message);

        await subscriber.PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal), serializedMessage);
    }

}