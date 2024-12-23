using Models;

namespace ApplicationLogic.Dtos;

public class UserInWorkspaceDto {
    public Guid WorkspaceId { get; set; }
    public required string Username { get; set; }
    public PermissionLevel Permission { get; set; }
}