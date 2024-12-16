using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryCassandra
{

    /// <summary>
    /// Read document info from Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public Document ReadDocumentInfo()
    {

    }

    /// <summary>
    /// Read update from Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public UpdatesBySnapshot ReadUpdate()
    {

    }

    /// <summary>
    /// Read merged update from Redis pubsub
    /// </summary>
    /// <param name="document"></param>
    public string ReadMergedUpdate()
    {

    }

    /// <summary>
    /// Read snapshot from Redis pubsub 
    /// </summary>
    /// <param name="document"></param>
    public string ReadSnapshot()
    {

    }

    /// <summary>
    /// Read document's new name from Redis pubsub
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public (Guid, string) ReadDocumentName()
    {

    }

    /// <summary>
    /// Read document's ID from pubsub
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public Guid ReadDocumentID()
    {

    }

}
