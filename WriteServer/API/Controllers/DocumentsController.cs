using ApplicationLogic.Dtos;
using Models;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase {

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
        Console.WriteLine($"Changing document {dto.WorkspaceId}/{dto.DocumentId} name to {dto.NewName}");
        await Task.Delay(10);
        return Ok();
    }

    [HttpPost("snapshots/{documentId}")]
    public async Task<ActionResult<Snapshot>> CreateSnapshot(Guid documentId) {
        Console.WriteLine($"Creating new snapshot for {documentId}");
        await Task.Delay(10);
        return Ok(new Snapshot("Testshot", DateTime.Now));
    }

}