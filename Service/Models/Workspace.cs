namespace Models;

public class Workspace {
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
    public required string WorkspaceName { get; set; }
    public required DateTime CreatedAt { get; set; }
}
