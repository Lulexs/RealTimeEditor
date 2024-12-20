namespace Persistence.DocumentRepository;

public class DocumentRepositoryCassandra {


    public async Task<bool> VerifyExistsAsync(Guid workspaceId, Guid documentId) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync("SELECT documentname FROM documents WHERE workspaceid = ? AND documentid = ?");
        var boundStatement = statement.Bind(workspaceId, documentId);
        var result = await session.ExecuteAsync(boundStatement);

        if (result.Any())
            return true;
        return false;
    }

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