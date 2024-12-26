using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace Persistence;

public class RedisSessionManager {
    private static IDatabase _database = null!;
    private static ISubscriber _subscriber = null!;
    private static readonly object LockObject = new();
    private static ConnectionMultiplexer _connection = null!;

    public static IDatabase GetDatabase() {
        if (_database == null) {
            lock (LockObject) {
                if (_database == null) {
                    InitializeConnection();
                    _database = _connection.GetDatabase();
                }
            }
        }
        return _database;
    }

    public static ISubscriber GetSubscriber() {
        if (_subscriber == null) {
            lock (LockObject) {
                if (_subscriber == null) {
                    InitializeConnection();
                    _subscriber = _connection.GetSubscriber();
                }
            }
        }
        return _subscriber;
    }

    private static void InitializeConnection() {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<RedisSessionManager>();
        var configuration = builder.Build();

        var redisConnectionString = configuration["RedisConnectionString"];
        var redisPassword = configuration["RedisPassword"];
        //string redisPassword = "";

        var redisConfig = ConfigurationOptions.Parse(redisConnectionString!);
        //var redisConfig = ConfigurationOptions.Parse("localhost:6379");
        if (!string.IsNullOrWhiteSpace(redisPassword)) {
            redisConfig.Password = redisPassword;
        }

        _connection = ConnectionMultiplexer.Connect(redisConfig);
    }
}
