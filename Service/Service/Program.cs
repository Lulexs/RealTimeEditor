using StackExchange.Redis;
using Persistence;
using ApplicationLogic;

public class Program {
    public static async Task Main(string[] args) {
        ISubscriber subscriber = RedisSessionManager.GetSubscriber();

        await subscriber.SubscribeAsync(new RedisChannel("updates", RedisChannel.PatternMode.Literal),
            async (redisChannel, message) => {
                var updateLogic = new UpdateLogic(new Persistence.DocumentRepository.DocumentRepositoryCassandra());
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
