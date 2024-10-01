using Microsoft.Extensions.Caching.Distributed;
using NeoSmart.Caching.Sqlite;

namespace Tgvs.Cache
{
    public class TgvsSqliteCache(SqliteCache sqliteCache) : IDistributedCache
    {
        private readonly SqliteCache _sqliteCache = sqliteCache;

        public byte[]? Get(string key)
        {
            return _sqliteCache.Get(key);
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            return _sqliteCache.GetAsync(key, token);
        }

        public void Refresh(string key)
        {
            _sqliteCache.Refresh(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
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
    }
}
