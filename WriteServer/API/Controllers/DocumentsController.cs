using ApplicationLogic;
using ApplicationLogic.Dtos;
using ApplicationLogic.Exceptions;
using Models;
using Persistence;
using YDotNet.Document;
using YDotNet.Document.Transactions;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase {
    private readonly DocumentLogic _documentLogic;
    private readonly LockLogic _lockLogic;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(DocumentLogic documentLogic, LockLogic lockLogic, ILogger<DocumentsController> logger) {
        _documentLogic = documentLogic;
        _logger = logger;
        _lockLogic = lockLogic;
    }

    [HttpGet("{workspaceId}")]
    public async Task<ActionResult<List<Document>>> GetDocumentsInWorkspace(Guid workspaceId) {
        try {
            var session = CassandraSessionManager.GetSession();
            var documentStatement = await session.PrepareAsync(
                "SELECT * FROM documents WHERE workspaceid = ?"
            );
            var documentBoundStatement = documentStatement.Bind(workspaceId);
            var documentResult = (await session.ExecuteAsync(documentBoundStatement)).ToList();

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
            if (documents.Count == 0) {
                return NotFound($"No documents found for workspace {workspaceId}.");
            }

            _logger.LogInformation($"Retrieved {documents.Count} documents with snapshots for workspace {workspaceId}");
            return Ok(documents);
        }
        catch (Exception ex) {
            _logger.LogError($"Error during GetDocumentsInWorkspace: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving documents.");
        }
    }

    [HttpPost("")]
    public async Task<ActionResult<Document>> CreateDocument([FromBody] CreateDocumentDto dto) {
        try {
            var session = CassandraSessionManager.GetSession();
            var documentId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var statement = await session.PrepareAsync(
                "INSERT INTO documents (workspaceid, documentid, documentname, creatorusername, createdat, snapshot1) " +
                "VALUES (?, ?, ?, ?, ?, ?)"
            );
            var boundStatement = statement.Bind(dto.WorkspaceId, documentId, dto.DocumentName, dto.CreatorUsername, createdAt, createdAt);
            await session.ExecuteAsync(boundStatement);

            var newDocument = new Document {
                WorkspaceId = dto.WorkspaceId,
                DocumentId = documentId,
                DocumentName = dto.DocumentName,
                CreatorUsername = dto.CreatorUsername,
                CreatedAt = createdAt,
                SnapshotIds = new List<Snapshot> { new Snapshot("snapshot1", DateTime.UtcNow) }
            };

            _logger.LogInformation($"Document {documentId} created in workspace {dto.WorkspaceId} with name {dto.DocumentName}");
            return Ok(newDocument);
        }
        catch (Exception ex) {
            _logger.LogError($"Error during CreateDocument: {ex.Message}");
            return StatusCode(500, "An error occurred while creating the document.");
        }
    }

    [HttpDelete("{workspaceId}/{documentId}/{actionPerformer}")]
    public async Task<ActionResult> DeleteDocument(Guid workspaceId, Guid documentId, string actionPerformer) {
        try {
            var session = CassandraSessionManager.GetSession();
            var userStatement = await session.PrepareAsync(
                "SELECT permissionlevel FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
            );
            var userBoundStatement = userStatement.Bind(workspaceId, actionPerformer);
            var userResultSet = await session.ExecuteAsync(userBoundStatement);

            var userRow = userResultSet.FirstOrDefault();
            if (userRow == null) {
                return NotFound($"User {actionPerformer} not found in workspace {workspaceId}.");
            }
            var permissionLevel = (PermissionLevel)userRow.GetValue<int>("permissionlevel");
            if (permissionLevel != PermissionLevel.OWNER) {
                return Forbid($"User {actionPerformer} is not authorized to delete documents in workspace {workspaceId}.");
            }

            // snapshotovi vezani za dokument
            var snapshotStatement = await session.PrepareAsync(
                "DELETE FROM updates_by_snapshot WHERE documentid = ?"
            );
            var snapshotBoundStatement = snapshotStatement.Bind(documentId);
            await session.ExecuteAsync(snapshotBoundStatement);

            // brisi dokument finally
            var documentStatement = await session.PrepareAsync(
                "DELETE FROM documents WHERE workspaceid = ? AND documentid = ?"
            );
            var documentBoundStatement = documentStatement.Bind(workspaceId, documentId);
            await session.ExecuteAsync(documentBoundStatement);

            _logger.LogInformation($"Document {documentId} in workspace {workspaceId} and its snapshots deleted by {actionPerformer}");
            return Ok($"Document {documentId} and all its snapshots were deleted successfully.");
        }
        catch (Exception ex) {
            _logger.LogError($"Error during DeleteDocument: {ex.Message}");
            return StatusCode(500, "An error occurred while deleting the document.");
        }
    }

    [HttpPost("lock")]
    public async Task<ActionResult> AcquireLockForChangeDocumentName([FromBody] ChangeDocumentNameDto dto) {
        try {
            var resourceKey = $"{dto.WorkspaceId}-{dto.DocumentId}-changeDocumentName";
            await _lockLogic.LockResource(resourceKey);
            return Ok();
        }
        catch (LockTakenException e) {
            _logger.LogInformation("{}", e.Message);
            return StatusCode(409, "Another user is changing the document name");
        }
        catch (Exception e) {
            _logger.LogError("Error during acquire lock for change document name: {}", e.Message);
            return StatusCode(500, "An error occurred while attepmting to change the document name.");
        }
    }

    [HttpPut("")]
    public async Task<ActionResult> ChangeDocumentName([FromBody] ChangeDocumentNameDto dto) {
        try {
            await _documentLogic.ChangeDocumentName(dto);
            return Ok();
        }
        catch (DocumentNotFoundException e) {
            _logger.LogInformation("Change document name failed due to {}", e.Message);
            return NotFound(e.Message);
        }
        catch (Exception e) {
            _logger.LogError("Error during ChangeDocumentName: {}", e.Message);
            return StatusCode(500, "An error occurred while changing the document name.");
        }
    }


    [HttpPost("snapshots/{workspaceId}/{documentId}")]
    public async Task<ActionResult<Snapshot>> CreateSnapshot(Guid workspaceId, Guid documentId) {
        Console.WriteLine("Here");
        var session = CassandraSessionManager.GetSession();
        var documentStatement = await session.PrepareAsync(
            "SELECT * FROM documents WHERE workspaceid = ? AND documentid = ?"
        );
        var documentBoundStatement = documentStatement.Bind(workspaceId, documentId);
        var documentResult = (await session.ExecuteAsync(documentBoundStatement)).FirstOrDefault();

        Console.WriteLine("Here1");
        if (documentResult == null) {
            return NotFound("Document not found");
        }
        Console.WriteLine("Here2");

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

        Console.WriteLine(columnName);

        var updatesStatement = await session.PrepareAsync(
            "SELECT payload FROM updates_by_snapshot WHERE documentid = ? and snapshotid = ?"
        );
        var updatesStatementBound = updatesStatement.Bind(documentId, "snapshot1");
        var updates = (await session.ExecuteAsync(updatesStatementBound)).ToList();

        Doc doc = new();
        foreach (var row in updates) {
            var update = row.GetValue<byte[]>("payload");
            Transaction writeTransaction = doc.WriteTransaction();
            writeTransaction.ApplyV1(update);
            writeTransaction.Commit();
        }
        Transaction readTransaction = doc.ReadTransaction();
        var mergedUpdates = readTransaction.StateDiffV1([0]);
        readTransaction.Commit();

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

        return Ok(new Snapshot(columnName, DateTime.Now));
    }

    [HttpPost("snapshots")]
    public async Task<ActionResult<Document>> ForkSnapshot([FromBody] ForkSnapshotDto dto) {
        var session = CassandraSessionManager.GetSession();
        var documentId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var statement = await session.PrepareAsync(
            "INSERT INTO documents (workspaceid, documentid, documentname, creatorusername, createdat, snapshot1) " +
            "VALUES (?, ?, ?, ?, ?, ?)"
        );
        var boundStatement = statement.Bind(dto.WorkspaceId, documentId, dto.DocumentName, dto.Forker, createdAt, createdAt);
        await session.ExecuteAsync(boundStatement);

        var updatesStatement = await session.PrepareAsync(
             "SELECT payload FROM updates_by_snapshot WHERE documentid = ? and snapshotid = ?"
        );
        var updatesStatementBound = updatesStatement.Bind(dto.DocumentId, dto.SnapshotName);
        var updates = (await session.ExecuteAsync(updatesStatementBound)).ToList();

        Doc doc = new();
        foreach (var row in updates) {
            var update = row.GetValue<byte[]>("payload");
            Transaction writeTransaction = doc.WriteTransaction();
            writeTransaction.ApplyV1(update);
            writeTransaction.Commit();
        }
        Transaction readTransaction = doc.ReadTransaction();
        var mergedUpdates = readTransaction.StateDiffV1([0]);
        readTransaction.Commit();

        var newSnapshotStatement = await session.PrepareAsync(
            "INSERT INTO updates_by_snapshot(documentId, snapshotId, updateId, payload) VALUES (?, ?, ?, ?)"
        );
        var newSnapshotStatementBound = newSnapshotStatement.Bind(documentId, "snapshot1", (long)0, mergedUpdates);

        await session.ExecuteAsync(newSnapshotStatementBound);

        var newDocument = new Document {
            WorkspaceId = dto.WorkspaceId,
            DocumentId = documentId,
            DocumentName = dto.DocumentName,
            CreatorUsername = dto.Forker,
            CreatedAt = createdAt,
            SnapshotIds = new List<Snapshot> { new Snapshot("snapshot1", DateTime.UtcNow) }
        };

        return Ok(newDocument);
    }

}