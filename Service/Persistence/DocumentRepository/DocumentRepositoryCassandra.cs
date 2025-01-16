using Cassandra.Mapping;
using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryCassandra {

    /// <summary>
    /// Write document info from redis pubsub to Cassandra
    /// </summary>
    /// <param name="document"></param>
    public void CreateDocument(Document document) {

    }

    public async Task SaveUpdateAsync(UpdatesBySnapshot update) {
        var session = CassandraSessionManager.GetSession();
        var mapper = new Mapper(session);

        await mapper.InsertAsync(update);
    }

    /// <summary>
    /// Write merged update from Redis pubsub to Cassandra as new snapshot
    /// </summary>
    /// <param name="document"></param>
    public void SaveSnapshot(string snapshotPayload) {

    }

    /// <summary>
    /// Write snapshot from Redis pubsub to Cassandra as new document 
    /// </summary>
    /// <param name="document"></param>
    public void SaveSnapshotAsDocument(string snapshotPayload) {

    }

    public async Task ChangeDocumentName(Guid workspaceId, Guid documentId, string newName) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync("UPDATE documents SET documentname = ? WHERE workspaceid = ? AND documentid = ?");
        var boundStatement = statement.Bind(newName, workspaceId, documentId);

        await session.ExecuteAsync(boundStatement);
    }

    /// <summary>
    /// Delete document from Cassandra
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="newName"></param>
    public void DeleteDocument(Guid documentId) {

    }

}