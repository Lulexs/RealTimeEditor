using Cassandra;
using Microsoft.Extensions.Configuration;

namespace Persistence;

public class CassandraSessionManager {
    private static ISession _session = null!;
    private static readonly object LockObject = new();

    public static ISession GetSession() {
        if (_session == null) {
            lock (LockObject) {
                if (_session == null) {
                    var builder = new ConfigurationBuilder().AddUserSecrets<CassandraSessionManager>();
                    var configuration = builder.Build();
                    var secureConnectionBundle = configuration["SecureBundle"];
                    var clientId = configuration["ClientId"];
                    var clientSecret = configuration["ClientSecret"];

                    _session = Cluster.Builder()
                                      .WithCloudSecureConnectionBundle(secureConnectionBundle)
                                      .WithCredentials(clientId, clientSecret)
                                      .Build()
                                      .Connect();
                }
            }
        }

        return _session;
    }

}
