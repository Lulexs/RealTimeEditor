using Cassandra;

namespace Persistence;

internal class SessionManager {
    static ISession Session { get; set; } = null!;

    internal static ISession GetSession() {
        Session ??= Cluster.Builder()
                            .WithCloudSecureConnectionBundle(@"<C:\PATH\TO>\secure-connect-nbp-proj.zip")
                             //or if on linux .WithCloudSecureConnectionBundle(@"/PATH/TO/>secure-connect-nbp-proj.zip")
                             .WithCredentials("<CLIENT ID>", "<CLIENT SECRET>")
                             .Build()
                            .Connect();
        return Session;
    }
}