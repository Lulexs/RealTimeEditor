namespace ApplicationLogic.Dtos;

public class ChangeWorkspaceNameDto {
    public Guid WorkspaceId { get; set; }
    public required string NewName { get; set; }
}