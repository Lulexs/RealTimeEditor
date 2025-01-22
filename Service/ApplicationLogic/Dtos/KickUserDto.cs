namespace ApplicationLogic.Dtos;

public class KickUserDto {
    public Guid WorkspaceId { get; set; }
    public required string Username { get; set; }
    public required string Performer { get; set; }
}