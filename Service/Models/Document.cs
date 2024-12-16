namespace Models;

public class Document {

    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public required string DocumentName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required Guid UserId { get; set; }
    public required DateTime LastUpdate { get; set; }
    public required List<(string, DateTime)> SnapshotIds { get; set; }
}