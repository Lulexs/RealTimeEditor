using System.Text.Json;
using ApplicationLogic.Dtos;
using Microsoft.Extensions.Logging;
using Models;
using Persistence.UserRepository;
using ApplicationLogic.Utilities;
using ApplicationLogic.Exceptions;

namespace ApplicationLogic;
public class UserLogic {
    private readonly UserRepositoryCassandra _userRepoCass;
    private readonly UserRepositoryRedis _userRepoRed;
    private readonly ILogger<UserLogic> _logger;

    public UserLogic(UserRepositoryCassandra userRepoCass, UserRepositoryRedis userRepoRed,ILogger<UserLogic> logger) {
        _userRepoCass = userRepoCass;
        _userRepoRed = userRepoRed;
        _logger = logger;
    }

    public async Task<UserDto> LoginUserAsync(LoginDto loginDto) {
        var user = await _userRepoCass.GetUserByUsernameAsync(loginDto.Region, loginDto.Username);
        if (user == null) {
            throw new UserNotFoundException("User not found");
        }

        if(!CustomHasher.VerifyPassword(loginDto.Password, user.HashedPassword))
        {
            throw new InvalidUserPasswordException("Invalid password");
        }

        return new UserDto() {
            Region = user.Region,
            Username = user.Username,
            Avatar = user.Avatar
        };
    }

}