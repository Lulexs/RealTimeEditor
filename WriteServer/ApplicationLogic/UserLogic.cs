using ApplicationLogic.Dtos;
using Persistence.UserRepository;
using ApplicationLogic.Utilities;
using ApplicationLogic.Exceptions;
using Microsoft.Extensions.Options;

namespace ApplicationLogic;
public class UserLogic {
    private readonly UserRepositoryCassandra _userRepoCass;
    private readonly UserRepositoryRedis _userRepoRed;
    private readonly string _salt;

    public UserLogic(UserRepositoryCassandra userRepoCass, UserRepositoryRedis userRepoRed, IOptions<AppSettings> options) {
        _userRepoCass = userRepoCass;
        _userRepoRed = userRepoRed;
        _salt = options.Value.Salt;
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

}