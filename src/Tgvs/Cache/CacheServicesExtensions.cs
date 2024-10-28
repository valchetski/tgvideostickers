using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NeoSmart.Caching.Sqlite;

namespace Tgvs.Cache;

public static class CacheServicesExtensions
{
    public static IServiceCollection AddTgvsSqliteCache(this IServiceCollection services)
    {
        SQLitePCL.Batteries_V2.Init();

        return services
            .AddSingleton(Options.Create(new SqliteCacheOptions
            {
                // do not clean up cache automatically as it needs to be unexpired in TgvsSqliteCache
                CleanupInterval = null, 
            }))
            .AddSingleton<SqliteCache>()
            .AddSingleton(Options.Create(new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            }))
            .AddSingleton<IDistributedCache, TgvsSqliteCache>();
    }
}