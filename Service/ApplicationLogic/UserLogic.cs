using ApplicationLogic.Dtos;
using Persistence.UserRepository;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Models;

namespace ApplicationLogic;
public class UserLogic {
    private readonly UserRepositoryCassandra _userRepoCass;
    private readonly UserRepositoryRedis _userRepoRed;
    private readonly ILogger<UserLogic> _logger;

    public UserLogic(UserRepositoryCassandra userRepoCass, UserRepositoryRedis userRepoRed, ILogger<UserLogic> logger) {
        _userRepoCass = userRepoCass;
        _userRepoRed = userRepoRed;
        _logger = logger;
    }

    public async Task SaveUser(string? message) {
        if (message == null)
            return;

        RegisterDto? regDto;
        try {
            regDto = JsonSerializer.Deserialize<RegisterDto>(message);
            if (regDto == null) {
                _logger.LogWarning("Failed to deserialize message: {}", message);
                return;
            }
        }
        catch (JsonException ex) {
            _logger.LogError("Failed to deserialize message: {}", ex.Message);
            return;
        }

        try {
            await _userRepoCass.SaveUser(new User() {
                Region = regDto.Region,
                Username = regDto.Username,
                HashedPassword = regDto.Password,
                Email = regDto.Email,
                Avatar = regDto.Avatar,
                UserId = Guid.NewGuid()
            });
            _logger.LogInformation("User {Username} successfully saved to Cassandra.", regDto.Username);
        }
        catch (Exception ex) {
            _logger.LogError("Failed to save user {Username} to Cassandra: {Message}", regDto.Username, ex.Message);
        }
    }

}