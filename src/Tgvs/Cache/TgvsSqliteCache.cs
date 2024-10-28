using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NeoSmart.Caching.Sqlite;
using System.Reflection;
using Tgvs.Data;

namespace Tgvs.Cache;

public class TgvsSqliteCache(
    SqliteCache sqliteCache,
    IServiceProvider serviceProvider,
    IOptions<DistributedCacheEntryOptions> options,
    ILogger<TgvsSqliteCache> logger) : IDistributedCache
{
    private readonly DistributedCacheEntryOptions _options = options.Value;
    private bool _firstTimeRun = true;

    public byte[]? Get(string key)
    {
        FirstTimeWarmup();
        return sqliteCache.Get(key);
    }

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        FirstTimeWarmup();
        return sqliteCache.GetAsync(key, token);
    }

    public void Refresh(string key)
    {
        FirstTimeWarmup();
        sqliteCache.Refresh(key);
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        FirstTimeWarmup();
        return sqliteCache.RefreshAsync(key, token);
    }

    public void Remove(string key)
    {
        sqliteCache.Remove(key);
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return sqliteCache.RemoveAsync(key, token);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        sqliteCache.Set(key, value, options);
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        return sqliteCache.SetAsync(key, value, options, token);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
        Justification = "We had to use _db field to renew cache on first run. More details in comments below.")]
    private void FirstTimeWarmup()
    {
        if (!_firstTimeRun)
        {
            return;
        }

        _firstTimeRun = false;

        var db = sqliteCache
            .GetType()
            .GetField("_db", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(sqliteCache) as SqliteConnection ?? throw new InvalidOperationException("Can't get _db field to warmup cache");

        // a bit hacky implementation
        // free tier SQL database and free tier azure app service go to sleep after some time of inactivity
        // because of that first run may be slow
        // here we renew all cache entries
        using var renewCacheCommand = db.CreateCommand();
        renewCacheCommand.CommandText = 
            "UPDATE cache SET expiry = " + DateTimeOffset.UtcNow.Add(_options.AbsoluteExpirationRelativeToNow.GetValueOrDefault()).Ticks;
        renewCacheCommand.ExecuteNonQuery();

        Task.Run(() => 
        {
            using var scope = serviceProvider.CreateScope();
            var stickersDbContext = scope.ServiceProvider.GetRequiredService<StickersDbContext>();
                
            logger.LogInformation("Warm up database.");
            // while cache is unexpired we try to warm up database
            var count = stickersDbContext.Stickers.Count();
            logger.LogInformation("Warmed up database. Got {Count} Stickers", count);
        });
    }
}