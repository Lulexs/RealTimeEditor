using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryRedis {

    /// <summary>
    /// Read document info from cache
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public Document GetDocument(GUID workspaceId, GUID documentId) 
    {

    }

    /// <summary>
    /// Save document to cache
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public void SaveDocument(Document document) 
    {
        
    }

    /// <summary>
    /// Write document info to Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public void CreateDocument(Document document)
    {
        
    }

    /// <summary>
    /// Write update to Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public void SaveUpdate(GUID documentId, GUID updateId, string payload)
    {

    }

    /// <summary>
    /// Writes merged update to Redis pubsub as new snapshot
    /// </summary>
    /// <param name="document"></param>
    public void SaveSnapshot(GUID documentId, GUID updateId, string payload)
    {

    }

    /// <summary>
    /// Writes snapshot to Redis pubsub so it can be saved as new document
    /// </summary>
    /// <param name="document"></param>
    public void SaveSnapshotAsDocument(GUID documentId, GUID snapshotId)
    {

    }

    /// <summary>
    /// Write document's new name to Redis pubsub
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public void ChangeDocumentName(GUID documentId, string newName)
    {

    }

    /// <summary>
    /// Delete document with specified ID
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public void DeleteDocument(GUID documentId)
    {

    }

}