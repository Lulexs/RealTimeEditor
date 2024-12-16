using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryCassandra {

    /// <summary>
    /// Read document info from Cassandra
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public Document GetDocument(GUID workspaceId, GUID documentId) 
    {

    }

    /// <summary>
    /// Read snapshot that is read only
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public UpdatesBySnapshot GetSnapshot(GUID documentId, GUID snapshotId)
    {

    }
    
}