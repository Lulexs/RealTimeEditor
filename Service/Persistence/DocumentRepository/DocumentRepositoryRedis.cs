using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryRedis {


    public async Task UpdateCacheForDocument(Guid documentId, byte[] newContent) {
        var db = RedisSessionManager.GetDatabase();

        await db.SetAddAsync($"doc:{documentId}-snapshot1:content", newContent);
    }

    public async Task<byte[]?> ReadDocumentContent(Guid documentId) {
        var db = RedisSessionManager.GetDatabase();

        byte[]? content = await db.StringGetAsync($"doc:{documentId}-snapshot1:content");
        return content;
    }

    /// <summary>
    /// Read document info from Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public Document ReadDocumentInfo() {
        return null;
    }


    /// <summary>
    /// Read merged update from Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public string ReadMergedUpdate() {
        return null;
    }

    /// <summary>
    /// Read snapshot from Redis pubsub 
    /// </summary>
    /// <param name="document"></param>
    public string ReadSnapshot() {
        return null;
    }

    /// <summary>
    /// Read document's new name from Redis pubsub
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public (Guid, string) ReadDocumentName() {
        return (Guid.Empty, "");
    }

    /// <summary>
    /// Read document's ID from pubsub
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public Guid ReadDocumentID() {
        return Guid.Empty;
    }

}
