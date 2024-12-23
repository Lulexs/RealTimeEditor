namespace Models;

public class Document {

    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public required string DocumentName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string CreatorUsername { get; set; }
    public required List<Snapshot> SnapshotIds { get; set; }
}

public record Snapshot(string Name, DateTime CreatedAt);