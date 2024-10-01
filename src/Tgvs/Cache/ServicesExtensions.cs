using Microsoft.Extensions.Caching.Distributed;
using NeoSmart.Caching.Sqlite;

namespace Tgvs.Cache
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddTgvsSqliteCache(this IServiceCollection services)
        {
            SQLitePCL.Batteries_V2.Init();

            return services
                .AddSingleton<SqliteCache>()
                .AddSingleton<IDistributedCache, TgvsSqliteCache>();
        }
    }
}
