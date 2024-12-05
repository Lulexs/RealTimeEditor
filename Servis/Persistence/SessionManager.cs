using Cassandra;
using Microsoft.Extensions.Configuration;

namespace Persistence;

internal class SessionManager {
    static ISession Session { get; set; } = null!;

    internal static ISession GetSession() {
        if (Session == null) {

            var builder = new ConfigurationBuilder().AddUserSecrets<SessionManager>();
            var configuration = builder.Build();
            var SecureConnectionBundle = configuration["SecureBundle"];
            var ClientId = configuration["ClientId"];
            var ClientSecret = configuration["ClientSecret"];

            Session ??= Cluster.Builder()
                            .WithCloudSecureConnectionBundle(SecureConnectionBundle)
                             .WithCredentials(ClientId, ClientSecret)
                             .Build()
                            .Connect();

        }
        return Session;
    }
}