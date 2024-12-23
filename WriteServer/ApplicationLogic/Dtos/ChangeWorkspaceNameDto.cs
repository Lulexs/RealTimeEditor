namespace ApplicationLogic.Dtos;

public class ChangeWorkspaceNameDto {
    public Guid WorkspaceId { get; set; }
    public required string NewName { get; set; }
    public required string OwnerUsername { get; set; }
    public required string UserUsername { get; set; }

}