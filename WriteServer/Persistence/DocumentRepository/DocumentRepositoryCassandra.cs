using Models;

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

    public async Task UpdateDocumentNameAsync(Guid workspaceId, Guid documentId, string newName) {
        var session = CassandraSessionManager.GetSession();

        var statement = await session.PrepareAsync("UPDATE documents SET documentname = ? WHERE workspaceid = ? AND documentid = ?");
        var boundStatement = statement.Bind(newName, workspaceId, documentId);

        await session.ExecuteAsync(boundStatement);

    }

    public async Task<List<Guid>> GetDocumentIdsInWorkspace(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();
        var getDocumentsStatement = await session.PrepareAsync(
                        "SELECT documentid FROM documents WHERE workspaceid = ?"
                    );
        var getDocumentsBound = getDocumentsStatement.Bind(workspaceId);
        var documents = (await session.ExecuteAsync(getDocumentsBound))
            .Select(row => row.GetValue<Guid>("documentid"))
            .ToList();

        return documents;
    }

    public async Task DeleteDocumentUpdates(Guid documentId) {
        var session = CassandraSessionManager.GetSession();
        var deleteUpdatesStatement = await session.PrepareAsync(
                            "DELETE FROM updates_by_snapshot WHERE documentid = ?"
                        );
        var deleteUpdatesBound = deleteUpdatesStatement.Bind(documentId);
        await session.ExecuteAsync(deleteUpdatesBound);
    }

    public async Task DeleteDocuments(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();
        var deleteDocumentsStatement = await session.PrepareAsync(
                        "DELETE FROM documents WHERE workspaceid = ?"
                    );
        var deleteDocumentsBound = deleteDocumentsStatement.Bind(workspaceId);
        await session.ExecuteAsync(deleteDocumentsBound);
    }

    public async Task<List<Cassandra.Row>> GetDocumentsInWorkspace(Guid workspaceId) {
        var session = CassandraSessionManager.GetSession();
        var documentStatement = await session.PrepareAsync(
            "SELECT * FROM documents WHERE workspaceid = ?"
        );
        var documentBoundStatement = documentStatement.Bind(workspaceId);
        var documentResult = (await session.ExecuteAsync(documentBoundStatement)).ToList();
        return documentResult;
    }

    public async Task CreateDocumentAsync(Document doc) {
        var session = CassandraSessionManager.GetSession();
        var statement = await session.PrepareAsync(
            "INSERT INTO documents (workspaceid, documentid, documentname, creatorusername, createdat, snapshot1) " +
            "VALUES (?, ?, ?, ?, ?, ?)"
        );
        var boundStatement = statement.Bind(doc.WorkspaceId, doc.DocumentId, doc.DocumentName, doc.CreatorUsername, doc.CreatedAt, doc.CreatedAt);
        await session.ExecuteAsync(boundStatement);
    }

    public async Task<Cassandra.RowSet> GetUserPermissionLevel(Guid workspaceId, string username) {
        var session = CassandraSessionManager.GetSession();
        var userStatement = await session.PrepareAsync(
            "SELECT permissionlevel FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
        );
        var userBoundStatement = userStatement.Bind(workspaceId, username);
        var userResultSet = await session.ExecuteAsync(userBoundStatement);
        return userResultSet;
    }

    public async Task DeleteDocument(Guid workspaceId, Guid documentId) {
        var session = CassandraSessionManager.GetSession();
        var documentStatement = await session.PrepareAsync(
                        "DELETE FROM documents WHERE workspaceid = ? AND documentid = ?"
                    );
        var documentBoundStatement = documentStatement.Bind(workspaceId, documentId);
        await session.ExecuteAsync(documentBoundStatement);
    }

    public async Task<Cassandra.Row?> GetDocumentById(Guid workspaceId, Guid documentId) {
        var session = CassandraSessionManager.GetSession();
        var documentStatement = await session.PrepareAsync(
            "SELECT * FROM documents WHERE workspaceid = ? AND documentid = ?"
        );
        var documentBoundStatement = documentStatement.Bind(workspaceId, documentId);
        var documentResult = (await session.ExecuteAsync(documentBoundStatement)).FirstOrDefault();
        return documentResult;
    }

    public async Task<List<Cassandra.Row?>> GetUpdates(Guid documentId, string snapshotName) {
        var session = CassandraSessionManager.GetSession();
        var updatesStatement = await session.PrepareAsync(
            "SELECT payload FROM updates_by_snapshot WHERE documentid = ? and snapshotid = ?"
        );
        var updatesStatementBound = updatesStatement.Bind(documentId, snapshotName);
        var updates = (await session.ExecuteAsync(updatesStatementBound)).ToList();
        return updates;
    }

    public async Task CreateSnapshot(Guid workspaceId, Guid documentId, byte[]? mergedUpdates, string columnName) {
        var session = CassandraSessionManager.GetSession();
        var newSnapshotStatement = await session.PrepareAsync(
                    "INSERT INTO updates_by_snapshot(documentId, snapshotId, updateId, payload) VALUES (?, ?, ?, ?)"
                );
        var newSnapshotStatementBound = newSnapshotStatement.Bind(documentId, columnName, (long)0, mergedUpdates);
        await session.ExecuteAsync(newSnapshotStatementBound);

        var alterTable = await session.PrepareAsync(
            $"ALTER TABLE documents ADD {columnName} timestamp"
        );
        var alterTableBound = alterTable.Bind();
        try {
            await session.ExecuteAsync(alterTableBound);
        }
        catch (Exception) {

        }

        var updateSnapshotTime = await session.PrepareAsync(
            $"UPDATE documents SET {columnName} = ? where workspaceid = ? and documentid = ?"
        );
        var updateSnapshotTimeBound = updateSnapshotTime.Bind(DateTime.UtcNow, workspaceId, documentId);
        await session.ExecuteAsync(updateSnapshotTimeBound);
    }

    public async Task SaveUpdates(Guid documentId, byte[]? mergedUpdates) {
        var session = CassandraSessionManager.GetSession();
        var newSnapshotStatement = await session.PrepareAsync(
            "INSERT INTO updates_by_snapshot(documentId, snapshotId, updateId, payload) VALUES (?, ?, ?, ?)"
        );
        var newSnapshotStatementBound = newSnapshotStatement.Bind(documentId, "snapshot1", (long)0, mergedUpdates);
        await session.ExecuteAsync(newSnapshotStatementBound);
    }

}