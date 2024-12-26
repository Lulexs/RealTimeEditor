namespace ApplicationLogic.Dtos;

public class UserDto {
    public required string Region { get; set; }
    public required string Username { get; set; }
    public required string Avatar { get; set; }
}

public class LoginDto {
    public required string Region { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class RegisterDto {
    public required string Region { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Avatar { get; set; }
    public required string Email { get; set; }
}