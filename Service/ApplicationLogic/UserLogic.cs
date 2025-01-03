using ApplicationLogic.Dtos;
using Persistence.UserRepository;
using ApplicationLogic.Utilities;
using ApplicationLogic.Exceptions;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ApplicationLogic;
public class UserLogic {
    private readonly UserRepositoryCassandra _userRepoCass;
    private readonly UserRepositoryRedis _userRepoRed;
    private readonly string _salt;
    private readonly ILogger<UserLogic> _logger;

    public UserLogic(UserRepositoryCassandra userRepoCass, UserRepositoryRedis userRepoRed, IOptions<AppSettings> options, ILogger<UserLogic> logger) {
        _userRepoCass = userRepoCass;
        _userRepoRed = userRepoRed;
        _salt = options.Value.Salt;
        _logger = logger;
    }

    public async Task SaveUser(string? message){
        if(message == null)
            return;
        
        RegisterDto? regDto;
        try{
            regDto = JsonSerializer.Deserialize<RegisterDto>(message);
            if (regDto == null){
                _logger.LogWarning("Failed to deserialize message: {}", message);
                return;
                }
        }
        catch(JsonException ex){
                _logger.LogError("Failed to deserialize message: {}", ex.Message);
                return;
        }

        // prosledjujem RegisterDto u userRepoCass saveuser

        try{
            await _userRepoCass.SaveUser(regDto);
            _logger.LogInformation("User {Username} successfully saved to Cassandra.", user.Username);
        }
        catch(Exception ex){
            _logger.LogError("Failed to save user {Username} to Cassandra: {Message}", user.Username, ex.Message);
        }
    }

}