namespace Models;

public class UpdatesBySnapshot {
    public Guid DocumentId { get; set; }
    public required string SnapshotId { get; set; } //Yjs u svaki update upisuje id update (version vektor ili tako nesto), pa ne treba update_id da bude (t)uuid - u write servisu mora da se ekstraktuje taj id.
    public required long UpdateId { get; set; }
    public byte[] PayLoad { get; set; } = [];
}