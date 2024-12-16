namespace Models;

public class User {
    public required string Region { get; set; }
    public GUID UserId { get; set; }
    public required string Username { get; set; } 
    public required string Email { get; set; }
    public required string HashedPassword { get; set; }
    public string? Avatar { get; set; }        
}
