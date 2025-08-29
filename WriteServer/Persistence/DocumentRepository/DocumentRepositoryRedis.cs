using System.Text.Json;
using Models;
using StackExchange.Redis;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryRedis
{

    public async Task<bool> VerifyDocumentExistsAsync(Guid documentId)
    {
        var db = RedisSessionManager.GetDatabase();

        bool exists = await db.KeyExistsAsync($"doc:{documentId}:content");
        return exists;
    }

    public async Task<RedisValue[]> LoadCachedDocumentAsync(Guid docId, string snapshotId)
    {
        var db = RedisSessionManager.GetDatabase();

        RedisValue[] content = await db.SetMembersAsync($"doc:{docId}-{snapshotId}:content");

        return content;
    }

    public async Task CacheDocumentAsync(Guid docId, string snapshotId, byte[] content)
    {
        IDatabase database = RedisSessionManager.GetDatabase();

        await database.SetAddAsync($"doc:{docId}-{snapshotId}:content", content);
    }

    /// <summary>
    /// Write document info to Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public void CreateDocument(Document document)
    {

    }

    public async Task SaveUpdateAsync(Guid documentId, byte[] update)
    {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = $"realtimeupdate-{documentId}";
        var message = new
        {
            DocumentId = documentId,
            Update = Convert.ToBase64String(update)
        };
        string serializedMessage = JsonSerializer.Serialize(message);

        await subscriber.PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal), serializedMessage);
    }

    /// <summary>
    /// Writes merged update to Redis pubsub as new snapshot
    /// </summary>
    /// <param name="document"></param>
    public void SaveSnapshot(Guid documentId, Guid updateId, string payload)
    {

    }

    /// <summary>
    /// Writes snapshot to Redis pubsub so it can be saved as new document
    /// </summary>
    /// <param name="document"></param>
    public void SaveSnapshotAsDocument(Guid documentId, Guid snapshotId)
    {

    }

    public async Task ChangeDocumentName(Guid workspaceId, Guid documentId, string newName)
    {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = $"changedocname";
        var message = new
        {
            WorkspaceId = workspaceId,
            DocumentId = documentId,
            NewName = newName
        };
        string serializedMessage = JsonSerializer.Serialize(message);

        await subscriber.PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal), serializedMessage);
    }

    /// <summary>
    /// Delete document with specified ID
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public void DeleteDocument(Guid documentId)
    {

    }

}