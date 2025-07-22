using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;

public class RedLockManager : IDisposable {
    private readonly RedLockFactory _redLockFactory;

    private readonly ILogger<RedLockManager> _logger;

    private readonly ConnectionMultiplexer _redisConnection;

    public RedLockManager(ILogger<RedLockManager> logger) {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<RedLockManager>();
        var configuration = builder.Build();

        _logger = logger;

        var redisEndpoint = configuration["RedisEndpoint"] ?? "localhost";
        var redisPortValue = configuration["RedisPort"] ?? "6379";

        if (!int.TryParse(redisPortValue, out var redisPort)) {
            throw new InvalidOperationException("Invalid Redis port.");
        }

        var configurationOptions = new ConfigurationOptions {
            EndPoints = { { redisEndpoint, redisPort } },
            AbortOnConnectFail = false
        };

        _redisConnection = ConnectionMultiplexer.Connect(configurationOptions);

        var multiplexers = new List<RedLockMultiplexer> { _redisConnection };
        _redLockFactory = RedLockFactory.Create(multiplexers);

        _logger.LogInformation("RedLockManager has been initialized with Redis endpoint {RedisEndpoint}:{RedisPort}", redisEndpoint, redisPort);
    }

    public RedLockFactory GetFactory() {
        return _redLockFactory;
    }

    public void Dispose() {
        _logger.LogInformation("Disposing RedLockManager and releasing resources.");

        _redLockFactory.Dispose();
    }
}
