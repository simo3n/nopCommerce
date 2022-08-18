using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Nop.Core.Caching;
using Nop.Core.Configuration;

namespace Nop.Services.Caching;

public class MsSqlServerCacheManager : DistributedCacheManager
{
    private readonly DistributedCacheConfig _distributedCacheConfig;

    public MsSqlServerCacheManager(AppSettings appSettings, IDistributedCache distributedCache) : base(appSettings, distributedCache)
    {
        _distributedCacheConfig = appSettings.Get<DistributedCacheConfig>();
    }

    protected async Task PerformActionAsync(SqlCommand command, params SqlParameter [] parameters)
    {
        var conn = new SqlConnection(_distributedCacheConfig.ConnectionString);
        try
        {
            conn.Open();
            command.Connection = conn;
            if(parameters.Any())
                command.Parameters.AddRange(parameters);

            await  command.ExecuteNonQueryAsync();
        }
        finally
        {
            conn.Close();
        }
    }

    public override async Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
    {
        prefix = PrepareKeyPrefix(prefix, prefixParameters);
        var command =
            new SqlCommand(
                $"DELETE FROM {_distributedCacheConfig.SchemaName}.{_distributedCacheConfig.TableName} WHERE Id LIKE @Prefix + '%'");

        await PerformActionAsync(command, new SqlParameter("Prefix", SqlDbType.NVarChar) { Value = prefix });

        await RemoveByPrefixInstanceDataAsync(prefix);
    }

    public override async Task ClearAsync()
    {
        var command =
            new SqlCommand($"TRUNCATE TABLE {_distributedCacheConfig.SchemaName}.{_distributedCacheConfig.TableName}");

        await PerformActionAsync(command);

        ClearInstanceData();
    }
}