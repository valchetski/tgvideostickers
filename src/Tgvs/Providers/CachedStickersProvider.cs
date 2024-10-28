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
    private readonly IStickersProvider _originalProvider = originalProvider;
    private readonly IDistributedCache _cache = cache;
    private readonly DistributedCacheEntryOptions _options = options.Value;

    public async Task<Sticker[]?> GetStickersAsync()
    {
        var cachedStickers = await _cache.GetStringAsync("Stickers");
        Sticker[]? stickers;
        if (string.IsNullOrEmpty(cachedStickers))
        {
            stickers = await _originalProvider.GetStickersAsync();
            await _cache.SetStringAsync("Stickers", JsonSerializer.Serialize(stickers), _options);
        }
        else
        {
            stickers = JsonSerializer.Deserialize<Sticker[]>(cachedStickers);
        }

        return stickers;
    }
}
