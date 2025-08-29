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
}
