using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using System;
using System.Collections.Generic;
using System.Net;

public class RedLockManager : IDisposable
{
    private readonly RedLockFactory _redLockFactory;

    private readonly ILogger<RedLockManager> _logger;


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


        var redisEndpoints = new List<RedLockEndPoint>
        {
            new DnsEndPoint(redisEndpoint, redisPort)
        };

        
        _redLockFactory = RedLockFactory.Create(redisEndpoints);
        _logger.LogInformation("RedLockManager has been initialized with Redis endpoint {RedisEndpoint}:{RedisPort}", redisEndpoint, redisPort);

    }

    public RedLockFactory GetFactory()
    {
        return _redLockFactory;
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RedLockManager and releasing resources.");

        _redLockFactory.Dispose(); // Oslobađa sve Redis konekcije
    }
}
