namespace ApplicationLogic.Dtos;

public class ForkSnapshotDto {
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public required string DocumentName { get; set; }
    public required string SnapshotName { get; set; }
    public required string Forker { get; set; }
}