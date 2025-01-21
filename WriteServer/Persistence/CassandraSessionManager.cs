using Cassandra;
using Cassandra.Mapping;
using Microsoft.Extensions.Configuration;
using Models;

namespace Persistence;
// TODO VRATI NA INTERNAL DULEEE
public class CassandraSessionManager {
    private static ISession _session = null!;
    private static readonly object LockObject = new();
    //I OVO
    public static ISession GetSession() {
        if (_session == null) {
            lock (LockObject) {
                if (_session == null) {
                    var builder = new ConfigurationBuilder().AddUserSecrets<CassandraSessionManager>();
                    var configuration = builder.Build();
                    var secureConnectionBundle = configuration["SecureBundle"];
                    var clientId = configuration["ClientId"];
                    var clientSecret = configuration["ClientSecret"];

                    var updatesBySnapshotMap = new Map<UpdatesBySnapshot>()
                                                    .TableName("updates_by_snapshot")
                                                    .PartitionKey(u => u.DocumentId)
                                                    .ClusteringKey(u => u.SnapshotId)
                                                    .ClusteringKey(u => u.UpdateId)
                                                    .Column(u => u.PayLoad, cm => cm.WithName("payload"));

                    MappingConfiguration.Global.Define(updatesBySnapshotMap);

                    _session = Cluster.Builder()
                                      .WithCloudSecureConnectionBundle(secureConnectionBundle)
                                      .WithCredentials(clientId, clientSecret)
                                      .Build()
                                      .Connect("realtimeeditor");
                }
            }
        }

        return _session;
    }

}
