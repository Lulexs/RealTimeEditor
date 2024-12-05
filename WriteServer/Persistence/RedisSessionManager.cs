using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace Persistence;

public class RedisSessionManager {
    private static IDatabase _database = null!;
    private static readonly object LockObject = new();

    public static IDatabase GetDatabase() {
        if (_database == null) {
            lock (LockObject) {
                if (_database == null) {
                    var builder = new ConfigurationBuilder()
                        .AddUserSecrets<RedisSessionManager>();
                    var configuration = builder.Build();

                    var redisConnectionString = configuration["RedisConnectionString"];
                    var redisPassword = configuration["RedisPassword"];

                    var redisConfig = ConfigurationOptions.Parse(redisConnectionString!);
                    if (!string.IsNullOrWhiteSpace(redisPassword)) {
                        redisConfig.Password = redisPassword;
                    }

                    var connection = ConnectionMultiplexer.Connect(redisConfig);
                    _database = connection.GetDatabase();
                }
            }
        }

        return _database;
    }
}
