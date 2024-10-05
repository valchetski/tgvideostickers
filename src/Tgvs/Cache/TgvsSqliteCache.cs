using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NeoSmart.Caching.Sqlite;
using System.Reflection;
using Tgvs.Data;

namespace Tgvs.Cache
{
    public class TgvsSqliteCache(
        SqliteCache sqliteCache,
        IServiceProvider serviceProvider,
        IOptions<DistributedCacheEntryOptions> options) : IDistributedCache
    {
        private readonly SqliteCache _sqliteCache = sqliteCache;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly DistributedCacheEntryOptions _options = options.Value;
        private bool _firstTimeRun = true;

        public byte[]? Get(string key)
        {
            FirstTimeWarmup();
            return _sqliteCache.Get(key);
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            FirstTimeWarmup();
            return _sqliteCache.GetAsync(key, token);
        }

        public void Refresh(string key)
        {
            FirstTimeWarmup();
            _sqliteCache.Refresh(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            FirstTimeWarmup();
            return _sqliteCache.RefreshAsync(key, token);
        }

        public void Remove(string key)
        {
            _sqliteCache.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return _sqliteCache.RemoveAsync(key, token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _sqliteCache.Set(key, value, options);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return _sqliteCache.SetAsync(key, value, options, token);
        }

        private void FirstTimeWarmup()
        {
            if (_firstTimeRun)
            {
                _firstTimeRun = false;

                var db = _sqliteCache
                    .GetType()
                    .GetField("_db", BindingFlags.NonPublic | BindingFlags.Instance)?
                    .GetValue(_sqliteCache) as SqliteConnection ?? throw new InvalidOperationException("Can't get _db field to warmup cache");

                // a bit hacky implementation
                // free tier SQL database and free tier azure app service go to sleep after some time of inactivity
                // because of that first run may be slow
                // here we unexpire all cache entries
                using var unexpireCacheCommand = db.CreateCommand();
                unexpireCacheCommand.CommandText = 
                    "UPDATE cache SET expiry = " + (DateTimeOffset.UtcNow.Add(_options.AbsoluteExpirationRelativeToNow.GetValueOrDefault())).Ticks;
                unexpireCacheCommand.ExecuteNonQuery();

                Task.Run(() => 
                {
                    using var scope = _serviceProvider.CreateScope();
                    var stickersDbContext = scope.ServiceProvider.GetRequiredService<StickersDbContext>();
                    
                    // while cache is unexpired we try to warm up database
                    stickersDbContext.Stickers.FirstOrDefault();
                });
            }
        }
    }
}
