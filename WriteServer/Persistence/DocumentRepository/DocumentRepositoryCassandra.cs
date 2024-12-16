using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryCassandra {

    /// <summary>
    /// Read document info from Cassandra
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public Document GetDocument(Guid workspaceId, Guid documentId) {
        return null;
    }

    /// <summary>
    /// Read snapshot that is read only
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public UpdatesBySnapshot GetSnapshot(Guid documentId, Guid snapshotId) {
        return null;
    }

}