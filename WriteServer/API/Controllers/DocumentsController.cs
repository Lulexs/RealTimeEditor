using ApplicationLogic;
using ApplicationLogic.Dtos;
using ApplicationLogic.Exceptions;
using Models;

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
        Console.WriteLine($"Getting documents for workspace ${workspaceId}");
        await Task.Delay(10);
        var documents = Enumerable.Range(1, 4).Select(index => new Document {
            WorkspaceId = workspaceId,
            DocumentId = Guid.NewGuid(),
            DocumentName = $"Document_{index}",
            CreatedAt = DateTime.UtcNow.AddDays(-index),
            CreatorUsername = $"user{index}",
            SnapshotIds = [new("snapshot1", DateTime.Now), new("snapshot2", DateTime.Now), new("snapshot3", DateTime.Now)]
        }).ToList();

        documents[0].WorkspaceId = Guid.Parse("bb4f9ca1-41ec-469c-bbc8-666666666666");
        documents[0].DocumentId = Guid.Parse("0d49e653-9d02-4339-98a2-f122222425b2");
        documents[1].WorkspaceId = Guid.Parse("f6f0b661-8dd7-4122-954e-561621f92bbd");
        documents[1].DocumentId = Guid.Parse("d8728b9a-da67-420d-832a-5e855d84274b");

        return Ok(documents);
    }

    [HttpPost("")]
    public async Task<ActionResult<Document>> CreateDocument([FromBody] CreateDocumentDto dto) {
        Console.WriteLine($"Createing document for workspace ${dto.WorkspaceId} with name ${dto.DocumentName}");
        await Task.Delay(10);
        var doc = new Document() {
            WorkspaceId = dto.WorkspaceId,
            DocumentId = Guid.NewGuid(),
            DocumentName = dto.DocumentName,
            CreatorUsername = dto.CreatorUsername,
            CreatedAt = DateTime.Now,
            SnapshotIds = [new("snapshot1", DateTime.Now)]
        };
        return Ok(doc);
    }

    [HttpDelete("{workspaceId}/{documentId}/{actionPerformer}")]
    public async Task<ActionResult> DeleteDocument(Guid workspaceId, Guid documentId, string actionPerformer) {
        Console.WriteLine($"{actionPerformer} deleting document {workspaceId}/{documentId}");
        await Task.Delay(10);
        return Ok();
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


    [HttpPost("snapshots/{documentId}")]
    public async Task<ActionResult<Snapshot>> CreateSnapshot(Guid documentId) {
        Console.WriteLine($"Creating new snapshot for {documentId}");
        await Task.Delay(10);
        return Ok(new Snapshot("Testshot", DateTime.Now));
    }

    [HttpPost("snapshots")]
    public async Task<ActionResult<Document>> ForkSnapshot([FromBody] ForkSnapshotDto dto) {
        Console.WriteLine($"Forking snapshot {dto.SnapshotName} to document {dto.DocumentName}");
        await Task.Delay(10);
        var doc = new Document() {
            WorkspaceId = dto.WorkspaceId,
            DocumentId = Guid.NewGuid(),
            DocumentName = dto.DocumentName,
            CreatorUsername = dto.Forker,
            CreatedAt = DateTime.Now,
            SnapshotIds = [new("snapshot1", DateTime.Now)]
        };
        return Ok(doc);
    }

}