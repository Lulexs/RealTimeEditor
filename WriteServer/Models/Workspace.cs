namespace Models;

public class Workspace {
    public GUID UserId { get; set; }
    public GUID WorkspaceId { get; set;}
    public required string WorkspaceName { get; set; }
    public required DateTime CreatedAt { get; set;}
}
