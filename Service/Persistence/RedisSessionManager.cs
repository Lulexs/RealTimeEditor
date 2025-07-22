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
                    InitializeConnectionForLocal();
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
                    InitializeConnectionForLocal();
                    _subscriber = _connection.GetSubscriber();
                }
            }
        }
        return _subscriber;
    }

    // private static void InitializeConnectionForAzure() {
    //     var builder = new ConfigurationBuilder()
    //         .AddUserSecrets<RedisSessionManager>();
    //     var configuration = builder.Build();

    //     var redisConnectionString = configuration["RedisConnectionString"];
    //     var redisPassword = configuration["RedisPassword"];

    //     var redisConfig = ConfigurationOptions.Parse(redisConnectionString!);
    //     if (!string.IsNullOrWhiteSpace(redisPassword)) {
    //         redisConfig.Password = redisPassword;
    //     }

    //     _connection = ConnectionMultiplexer.Connect(redisConfig);
    // }

    private static void InitializeConnectionForLocal() {
        var redisConfig = ConfigurationOptions.Parse("localhost:6379");
        _connection = ConnectionMultiplexer.Connect(redisConfig);
    }

    private static void InitializeConnectionForRedisCloud() {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<RedisSessionManager>();
        var configuration = builder.Build();

        var redisEndpoint = configuration["RedisEndpoint"];
        var redisPort = int.Parse(configuration["RedisPort"]!);
        var redisPassword = configuration["RedisPassword"];
        var redisUser = configuration["RedisUser"];

        _connection = ConnectionMultiplexer.Connect(
            new ConfigurationOptions {
                EndPoints = { { redisEndpoint!, redisPort } },
                User = redisUser,
                Password = redisPassword
            }
        );
    }
}

