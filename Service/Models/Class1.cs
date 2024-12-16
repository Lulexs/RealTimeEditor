namespace Models;

public enum PermissionLevel {
    VIEW_ONLY,
    EDIT,
    ADMIN,
    OWNER
}

public class User {
    public required string Region { get; set; }
    public GUID UserId { get; set; }
    public required string Username { get; set; } 
    public required string Email { get; set; }
    public required string HashedPassword { get; set; }
    public string? Avatar { get; set; }        
}

public class Workspace {
    public GUID UserId { get; set; }
    public GUID WorkspaceId { get; set;}
    public required string WorkspaceName { get; set; }
    public required DateTime CreatedAt { get; set;}
}

public class UserByWorkspace {
    public GUID WorkspaceId { get; set; }
    public GUID UserId { get; set; }
    public required string Username { get; set; }
    public required PermissionLevel Permission { get; set; }
}

public class Document {

    public GUID WorkspaceId { get; set; }
    public GUID DocumentId { get; set; }
    public required string DocumentName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required GUID UserId { get; set; }
    public required DateTime LastUpdate { get; set; }
    public required List<(string, DateTime)> SnapshotIds { get; set; }
}

public class UpdatesBySnapshot {
    public GUID DocumentId { get; set; }
    public required GUID SnapshotId { get; set; } //Yjs u svaki update upisuje id update (version vektor ili tako nesto), pa ne treba update_id da bude (t)uuid - u write servisu mora da se ekstraktuje taj id.
    public required string UpdateId { get; set; }
    public string? PayLoad { get; set; }// ??
}