namespace Models;

public class Workspace {
    public required string Username { get; set; }
    public Guid WorkspaceId { get; set; }
    public required string WorkspaceName { get; set; }
    public required string OwnerUsername { get; set; }
    public PermissionLevel Permission { get; set; }
    public required DateTime CreatedAt { get; set; }
}
