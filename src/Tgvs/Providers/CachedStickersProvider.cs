using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Tgvs.Providers;

public class CachedStickersProvider(
    IStickersProvider originalProvider,
    IDistributedCache cache,
    IOptions<DistributedCacheEntryOptions> options)
     : IStickersProvider
{
    private readonly DistributedCacheEntryOptions _options = options.Value;

    public async Task<Sticker[]?> GetStickersAsync()
    {
        var cachedStickers = await cache.GetStringAsync("Stickers");
        Sticker[]? stickers;
        if (string.IsNullOrEmpty(cachedStickers))
        {
            stickers = await originalProvider.GetStickersAsync();
            await cache.SetStringAsync("Stickers", JsonSerializer.Serialize(stickers), _options);
        }
        else
        {
            stickers = JsonSerializer.Deserialize<Sticker[]>(cachedStickers);
        }

        return stickers;
    }
}
