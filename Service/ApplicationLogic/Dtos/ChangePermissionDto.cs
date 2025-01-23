public class ChangePermissionDto
{
    public Guid WorkspaceId { get; set; }
    public string Username { get; set; }
    public int NewPermission { get; set; }
    public string Performer { get; set; }
}