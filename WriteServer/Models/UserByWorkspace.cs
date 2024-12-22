namespace Models;

public class UserByWorkspace {
    public Guid WorkspaceId { get; set; }
    public required string Username { get; set; }
    public required PermissionLevel Permission { get; set; }
}