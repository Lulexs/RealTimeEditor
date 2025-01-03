using ApplicationLogic.Dtos;
using Persistence.UserRepository;
using ApplicationLogic.Utilities;
using ApplicationLogic.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Models;

namespace ApplicationLogic;
public class UserLogic {
    private readonly UserRepositoryCassandra _userRepoCass;
    private readonly UserRepositoryRedis _userRepoRed;
    private readonly ILogger<UserLogic> _logger;
    private readonly string _salt;

    public UserLogic(UserRepositoryCassandra userRepoCass, UserRepositoryRedis userRepoRed, IOptions<AppSettings> options, ILogger<UserLogic> logger) {
        _userRepoCass = userRepoCass;
        _userRepoRed = userRepoRed;
        _salt = options.Value.Salt;
        _logger = logger;
    }

    public async Task<UserDto> LoginUserAsync(LoginDto loginDto) {
        var user = await _userRepoCass.GetUserByUsernameAsync(loginDto.Username) ?? throw new UserNotFoundException("User not found");

        if (!CustomHasher.VerifyPassword(loginDto.Password, user.HashedPassword, _salt)) {
            throw new InvalidUserPasswordException("Invalid password");
        }

        return new UserDto() {
            Region = user.Region,
            Username = user.Username,
            Avatar = user.Avatar!
        };
    }
    public async Task RegisterUserAsync(RegisterDto regDto) {
        if (regDto == null ||
            string.IsNullOrWhiteSpace(regDto.Username) ||
            string.IsNullOrWhiteSpace(regDto.Password) ||
            string.IsNullOrWhiteSpace(regDto.Region) ||
            string.IsNullOrWhiteSpace(regDto.Avatar) ||
            string.IsNullOrWhiteSpace(regDto.Email)) {
            throw new InvalidRegisterDataException("Invalid registration data provided, all fields are required for registration");
        }

        if (await _userRepoCass.GetUserByUsernameAsync(regDto.Username) != null) {
            throw new UserAlreadyExistsException("User already exists");
        }

        // Password is hashed in message in register channel
        var hashedPassword = CustomHasher.HashPassword(regDto.Password, _salt);
        var user = new User() {
            Username = regDto.Username,
            Email = regDto.Email,
            HashedPassword = hashedPassword,
            Region = regDto.Region,
            Avatar = regDto.Avatar
        };

        await _userRepoRed.SaveUser(user);
        _logger.LogInformation("User {Username} queued for registration in Redis pub-sub.", regDto.Username);
    }

}