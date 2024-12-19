using System.Text.Json;
using Models;
using StackExchange.Redis;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryRedis {

    /// <summary>
    /// Read document info from cache
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public Document GetDocument(Guid workspaceId, Guid documentId) {
        return null;
    }

    /// <summary>
    /// Save document to cache
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public void SaveDocument(Document document) {

    }

    /// <summary>
    /// Write document info to Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public void CreateDocument(Document document) {

    }


    /// <summary>
    /// Write update to Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public async Task SaveUpdateAsync(Guid documentId, byte[] update) {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = "updates";
        var message = new {
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
    public void SaveSnapshot(Guid documentId, Guid updateId, string payload) {

    }

    /// <summary>
    /// Writes snapshot to Redis pubsub so it can be saved as new document
    /// </summary>
    /// <param name="document"></param>
    public void SaveSnapshotAsDocument(Guid documentId, Guid snapshotId) {

    }

    /// <summary>
    /// Write document's new name to Redis pubsub
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public void ChangeDocumentName(Guid documentId, string newName) {

    }

    /// <summary>
    /// Delete document with specified ID
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public void DeleteDocument(Guid documentId) {

    }

}