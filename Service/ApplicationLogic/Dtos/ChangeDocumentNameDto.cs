namespace ApplicationLogic.Dtos;

public class ChangeDocumentNameDto {
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public required string NewName { get; set; }
}