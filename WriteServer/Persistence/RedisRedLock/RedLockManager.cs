using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;

public class RedLockManager : IDisposable
{
    private readonly RedLockFactory _redLockFactory;

    private readonly ILogger<RedLockManager> _logger;

    private readonly ConnectionMultiplexer _redisConnection;

    public RedLockManager(ILogger<RedLockManager> logger)
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<RedLockManager>();
        var configuration = builder.Build();


        _logger = logger;
        
        var redisEndpoint = configuration["RedisEndpoint"];
        if (string.IsNullOrEmpty(redisEndpoint))
        {
            throw new InvalidOperationException("Configuration for 'RedisEndpoint' is missing or empty. Please check your configuration.");
        }

        var redisPortValue = configuration["RedisPort"];
        if (string.IsNullOrEmpty(redisPortValue) || !int.TryParse(redisPortValue, out var redisPort))
        {
            throw new InvalidOperationException("Configuration for 'RedisPort' is missing, empty, or invalid. Please provide a valid integer value.");
        }


        var redisPassword = configuration["RedisPassword"];
        var redisUser = configuration["RedisUser"];

        var configurationOptions = new ConfigurationOptions
        {
           EndPoints = {{redisEndpoint, redisPort}},
           User = string.IsNullOrEmpty(redisUser) ? null : redisUser,
           Password = string.IsNullOrEmpty(redisPassword) ? null : redisPassword,
           AbortOnConnectFail = false 
        };

        _redisConnection = ConnectionMultiplexer.Connect(configurationOptions);

        var multiplexers = new List<RedLockMultiplexer>
        {
            _redisConnection
        };

        _redLockFactory = RedLockFactory.Create(multiplexers);
        _logger.LogInformation("RedLockManager has been initialized with Redis endpoint {RedisEndpoint}:{RedisPort}", redisEndpoint, redisPort);

    }

    public RedLockFactory GetFactory()
    {
        return _redLockFactory;
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RedLockManager and releasing resources.");

        _redLockFactory.Dispose(); 
    }
}
