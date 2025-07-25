﻿using StackExchange.Redis;
using Persistence;
using ApplicationLogic;
using Microsoft.Extensions.Logging;

public class Program {
    public static async Task Main(string[] args) {
        using var loggerFactory = LoggerFactory.Create(builder => {
            builder
                .AddConsole();
        });


        ISubscriber subscriber = RedisSessionManager.GetSubscriber();

        await subscriber.SubscribeAsync(new RedisChannel("realtimeupdate-*", RedisChannel.PatternMode.Pattern),
            async (redisChannel, message) => {
                var updateLogic = new UpdateLogic(new Persistence.DocumentRepository.DocumentRepositoryCassandra(),
                                                  new Persistence.DocumentRepository.DocumentRepositoryRedis(),
                                                  loggerFactory.CreateLogger<UpdateLogic>());
                await updateLogic.SaveUpdate(message);
            });

        await subscriber.SubscribeAsync(new RedisChannel("register", RedisChannel.PatternMode.Literal),
            async (redisChannel, message) => {
                var userLogic = new UserLogic(new Persistence.UserRepository.UserRepositoryCassandra(),
                                              new Persistence.UserRepository.UserRepositoryRedis(),
                                              loggerFactory.CreateLogger<UserLogic>());
                await userLogic.SaveUser(message);
            });
        await subscriber.SubscribeAsync(new RedisChannel("changedocname", RedisChannel.PatternMode.Literal),
            async (redisChannel, message) => {
                var docLogic = new DocumentLogic(new Persistence.DocumentRepository.DocumentRepositoryCassandra());
                await docLogic.ChangeDocumentName(message);
            }
        );
        await subscriber.SubscribeAsync(new RedisChannel("changeworkspacename", RedisChannel.PatternMode.Literal),
            async (redisChannel, message) => {
                var wsLogic = new WorkspaceLogic(new Persistence.WorkspaceRepository.WorkspaceRepositoryCassandra());

                await wsLogic.ChangeWorkspaceName(message);
            }
        );
        await subscriber.SubscribeAsync(new RedisChannel("kickuser", RedisChannel.PatternMode.Literal),
            async (redisChannel, message) => {
                var wsLogic = new WorkspaceLogic(new Persistence.WorkspaceRepository.WorkspaceRepositoryCassandra());

                await wsLogic.KickUserFromWorkspace(message);
            }
        );
        await subscriber.SubscribeAsync(new RedisChannel("changeuserpermission", RedisChannel.PatternMode.Literal),
            async (redisChannel, message) =>
            {
                var wsLogic = new WorkspaceLogic(new Persistence.WorkspaceRepository.WorkspaceRepositoryCassandra());

                await wsLogic.ApplyUserPermissionChange(message);
            }
        );

        Console.WriteLine("Press Ctrl+C to exit...");

        var exitEvent = new ManualResetEvent(false);
        Console.CancelKeyPress += (sender, e) => {
            Console.WriteLine("Exiting...");
            e.Cancel = true;
            exitEvent.Set();
        };
        exitEvent.WaitOne();
    }

}
