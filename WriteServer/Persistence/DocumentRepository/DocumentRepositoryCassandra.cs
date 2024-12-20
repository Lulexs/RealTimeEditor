using Cassandra.Mapping;
using Models;

namespace Persistence.DocumentRepository;

public class DocumentRepositoryCassandra {

    /// <summary>
    /// Read document info from Cassandra
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public Document? GetDocument(Guid workspaceId, Guid documentId) {
        return new Document() {
            WorkspaceId = workspaceId,
            DocumentId = documentId,
            DocumentName = "Doc123",
            CreatedAt = DateTime.Now,
            CreatorUserId = Guid.NewGuid(),
            SnapshotIds = []
        };
    }

    /// <summary>
    /// Read snapshot that is read only
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public async Task<List<byte[]>> GetSnapshot(Guid documentId, string snapshotId) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync("SELECT payload from updates_by_snapshot WHERE documentid = ? AND snapshotid = ?");
        var boundStatement = statement.Bind(documentId, snapshotId);
        var resultSet = await session.ExecuteAsync(boundStatement);

        List<byte[]> payloads = [];
        foreach (var row in resultSet) {
            var payload = row.GetValue<byte[]>("payload");
            payloads.Add(payload);
        }

        return payloads;
    }

}