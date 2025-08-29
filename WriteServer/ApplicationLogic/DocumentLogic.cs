using ApplicationLogic.Dtos;
using ApplicationLogic.Exceptions;
using Models;
using Persistence.DocumentRepository;
using YDotNet.Document;
using YDotNet.Document.Transactions;

namespace ApplicationLogic;

public class DocumentLogic {
    private readonly DocumentRepositoryCassandra _docRepoCass;
    private readonly DocumentRepositoryRedis _docRepoRed;

    public DocumentLogic(DocumentRepositoryCassandra docRepoCass, DocumentRepositoryRedis docRepoRed) {
        _docRepoCass = docRepoCass;
        _docRepoRed = docRepoRed;
    }

    public async Task ChangeDocumentName(ChangeDocumentNameDto dto) {
        var documentExists = await _docRepoCass.VerifyExistsAsync(dto.WorkspaceId, dto.DocumentId);
        if (!documentExists) {
            throw new DocumentNotFoundException($"Document {dto.DocumentId} not found");
        }
        await _docRepoRed.ChangeDocumentName(dto.WorkspaceId, dto.DocumentId, dto.NewName);
    }

    public async Task<List<Document>> GetDocumentsInWorkspace(Guid workspaceId) {
        var documentResult = await _docRepoCass.GetDocumentsInWorkspace(workspaceId);

        var documents = new List<Document>();

        foreach (var row in documentResult) {
            var snapshotIds = new List<Snapshot>();

            int index = 1;
            while (true) {
                string columnName = $"snapshot{index}";
                var column = row.GetColumn(columnName);

                if (column == null) {
                    break;
                }

                var snapshotTimestamp = row.GetValue<DateTime?>(columnName);
                if (snapshotTimestamp == null)
                    break;

                snapshotIds.Add(new(columnName, snapshotTimestamp.GetValueOrDefault()));
                index++;
            }

            var document = new Document {
                WorkspaceId = row.GetValue<Guid>("workspaceid"),
                DocumentId = row.GetValue<Guid>("documentid"),
                DocumentName = row.GetValue<string>("documentname"),
                CreatedAt = row.GetValue<DateTime>("createdat"),
                CreatorUsername = row.GetValue<string>("creatorusername"),
                SnapshotIds = snapshotIds
            };

            documents.Add(document);
        }

        return documents;
    }

    public async Task<Document> CreateDocument(CreateDocumentDto dto) {
        var document = new Document {
            WorkspaceId = dto.WorkspaceId,
            DocumentId = Guid.NewGuid(),
            DocumentName = dto.DocumentName,
            CreatorUsername = dto.CreatorUsername,
            CreatedAt = DateTime.UtcNow,
            SnapshotIds = new List<Snapshot> { new Snapshot("snapshot1", DateTime.UtcNow) }
        };
        await _docRepoCass.CreateDocumentAsync(document);
        return document;
    }

    public async Task DeleteDocument(Guid workspaceId, Guid documentId, string username) {
        var userResultSet = await _docRepoCass.GetUserPermissionLevel(workspaceId, username);

        var userRow = userResultSet.FirstOrDefault();
        if (userRow == null) {
            throw new Exception($"User {username} not found in workspace {workspaceId}.");
        }
        var permissionLevel = (PermissionLevel)userRow.GetValue<int>("permissionlevel");
        if (permissionLevel != PermissionLevel.OWNER) {
            throw new UnauthorizedAccessException($"User {username} is not authorized to delete documents in workspace {workspaceId}.");
        }

        await _docRepoCass.DeleteDocumentUpdates(documentId);

        await _docRepoCass.DeleteDocument(workspaceId, documentId);

    }

    public async Task<string> CreateSnapshot(Guid workspaceId, Guid documentId) {
        var documentResult = await _docRepoCass.GetDocumentById(workspaceId, documentId);
        if (documentResult == null) {
            throw new DocumentNotFoundException($"Document {documentId} not found");
        }

        int index = 1;
        string columnName;
        while (true) {
            columnName = $"snapshot{index}";
            var column = documentResult.GetColumn(columnName);

            if (column == null || documentResult.GetValue<DateTime?>(columnName) == null) {
                break;
            }

            index++;
        }

        var updates = await _docRepoCass.GetUpdates(documentId);

        Doc doc = new();
        foreach (var row in updates) {
            var update = row?.GetValue<byte[]>("payload");
            Transaction writeTransaction = doc.WriteTransaction();
            writeTransaction.ApplyV1(update!);
            writeTransaction.Commit();
        }
        Transaction readTransaction = doc.ReadTransaction();
        var mergedUpdates = readTransaction.StateDiffV1([0]);
        readTransaction.Commit();

        await _docRepoCass.CreateSnapshot(workspaceId, documentId, mergedUpdates, columnName);

        return columnName;
    }


}