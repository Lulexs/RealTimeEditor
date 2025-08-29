using Models;
using System.Text.Json;
using StackExchange.Redis;

namespace Persistence.UserRepository;

public class UserRepositoryRedis {

    public async Task SaveUser(User user) {
        var subscriber = RedisSessionManager.GetSubscriber();

        string channelName = "register";

        var message = new {
            user.UserId,
            user.Username,
            Password = user.HashedPassword,
            user.Region,
            user.Avatar,
            user.Email
        };

        string serializedMessage = JsonSerializer.Serialize(message);

        await subscriber.PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal), serializedMessage);
    }

}
