using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryRedis {

    /// <summary>
    /// Read document info from Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public Document ReadDocumentInfo() {
        return null;
    }

    /// <summary>
    /// Read update from Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public UpdatesBySnapshot ReadUpdate() {
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
