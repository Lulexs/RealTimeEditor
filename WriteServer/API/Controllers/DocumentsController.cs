using ApplicationLogic.Dtos;
using Models;
using Persistence.DocumentRepository;
namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase {

    private readonly RedLockManager _redLockManager;
    private readonly DocumentRepositoryCassandra _documentRepository;

    public DocumentsController(RedLockManager redLockManager, DocumentRepositoryCassandra documentRepository) {
        _redLockManager = redLockManager;
        _documentRepository = documentRepository;
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

    [HttpPut("")]
    public async Task<ActionResult> ChangeDocumentName([FromBody] ChangeDocumentNameDto dto) {
        var resourceKey = $"{dto.WorkspaceId}-{dto.DocumentId}-changeDocumentName";
        using (var redLock = await _redLockManager.GetFactory().CreateLockAsync(
            resourceKey,
            TimeSpan.FromSeconds(2))) {
            if (!redLock.IsAcquired) {
                return StatusCode(409, "Another user is changing the document name");
            }
            try {
                Console.WriteLine($"Lock succesfully acquired changing document name for {dto.WorkspaceId}/{dto.DocumentId} to {dto.NewName}");
                var documentExists = await _documentRepository.VerifyExistsAsync(dto.WorkspaceId, dto.DocumentId);
                if (!documentExists) {
                    return NotFound("Document does not exist.");
                }

                await _documentRepository.UpdateDocumentNameAsync(dto.WorkspaceId, dto.DocumentId, dto.NewName);
                
                Console.WriteLine($"Changed document {dto.WorkspaceId}/{dto.DocumentId} name to {dto.WorkspaceId}/{dto.NewName}");
                return Ok("Document name changed successfully.");
                

            }
            catch (Exception e) {
                Console.WriteLine($"Error during ChangeDocumentName: {e.Message}");
                return StatusCode(500, "An error occurred while changing the document name." + e.Message);
            }
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