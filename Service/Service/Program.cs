using StackExchange.Redis;
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

        await subscriber.SubscribeAsync(new RedisChannel("updates", RedisChannel.PatternMode.Literal),
            async (redisChannel, message) => {
                var updateLogic = new UpdateLogic(new Persistence.DocumentRepository.DocumentRepositoryCassandra(),
                                                  new Persistence.DocumentRepository.DocumentRepositoryRedis(),
                                                  loggerFactory.CreateLogger<UpdateLogic>());
                await updateLogic.SaveUpdate(message);
            });


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
