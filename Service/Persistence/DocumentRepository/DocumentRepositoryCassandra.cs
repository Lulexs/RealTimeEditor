using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryCassandra
{

    /// <summary>
    /// Write document info from redis pubsub to Cassandra
    /// </summary>
    /// <param name="document"></param>
    public void CreateDocument(Document document)
    {

    }

    /// <summary>
    /// Write update from Redis pubsub to Cassandra
    /// </summary>
    /// <param name="document"></param>
    public void SaveUpdate(UpdateByWorkspace update)
    {

    }

    /// <summary>
    /// Write merged update from Redis pubsub to Cassandra as new snapshot
    /// </summary>
    /// <param name="document"></param>
    public void SaveSnapshot(string snapshotPayload)
    {

    }

    /// <summary>
    /// Write snapshot from Redis pubsub to Cassandra as new document 
    /// </summary>
    /// <param name="document"></param>
    public void SaveSnapshotAsDocument(string snapshotPayload)
    {

    }

    /// <summary>
    /// Write document's new name from Redis pubsub Cassandra
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public void ChangeDocumentName(Guid documentId, string documentName)
    {

    }

    /// <summary>
    /// Delete document from Cassandra
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public void DeleteDocument(Guid documentId)
    {

    }

}