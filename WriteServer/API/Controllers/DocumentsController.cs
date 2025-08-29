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
            var documents = await _documentLogic.GetDocumentsInWorkspace(workspaceId);
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
            var document = await _documentLogic.CreateDocument(dto);

            _logger.LogInformation($"Document {document.DocumentId} created in workspace {document.WorkspaceId} with name {document.DocumentName}");
            return Ok(document);
        }
        catch (Exception ex) {
            _logger.LogError($"Error during CreateDocument: {ex.Message}");
            return StatusCode(500, "An error occurred while creating the document.");
        }
    }

    [HttpDelete("{workspaceId}/{documentId}/{actionPerformer}")]
    public async Task<ActionResult> DeleteDocument(Guid workspaceId, Guid documentId, string actionPerformer) {
        try {
            await _documentLogic.DeleteDocument(workspaceId, documentId, actionPerformer);

            _logger.LogInformation($"Document {documentId} in workspace {workspaceId} and its snapshots deleted by {actionPerformer}");
            return Ok($"Document {documentId} and all its snapshots were deleted successfully.");
        }
        catch (UnauthorizedAccessException ex) {
            _logger.LogWarning($"Unauthorized access during DeleteDocument: {ex.Message}");
            return Forbid("You do not have permission to delete this document.");
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
        var columnName = await _documentLogic.CreateSnapshot(workspaceId, documentId);
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