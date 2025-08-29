using Cassandra.Mapping;
using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryCassandra {

    public async Task SaveUpdateAsync(UpdatesBySnapshot update) {
        var session = CassandraSessionManager.GetSession();
        var mapper = new Mapper(session);

        await mapper.InsertAsync(update);
    }
    public async Task ChangeDocumentName(Guid workspaceId, Guid documentId, string newName) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync("UPDATE documents SET documentname = ? WHERE workspaceid = ? AND documentid = ?");
        var boundStatement = statement.Bind(newName, workspaceId, documentId);

        await session.ExecuteAsync(boundStatement);
    }

}