namespace ApplicationLogic.Dtos;

public class CreateDocumentDto {
    public Guid WorkspaceId { get; set; }
    public required string DocumentName { get; set; }
    public required string CreatorUsername { get; set; }
}