using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Tgvs.Providers;

public class CachedStickersProvider(IStickersProvider originalProvider, IDistributedCache cache)
     : IStickersProvider
{
    private readonly IStickersProvider _originalProvider = originalProvider;
    private readonly IDistributedCache _cache = cache;
    private readonly DistributedCacheEntryOptions _options = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
    };

    public async Task<Sticker[]?> GetStickersAsync()
    {
        var cachedStickers = await _cache.GetStringAsync("Stickers");
        Sticker[]? stickers;
        if (cachedStickers == null)
        {
            stickers = await _originalProvider.GetStickersAsync();
            await _cache.SetStringAsync("Stickers", JsonConvert.SerializeObject(stickers), _options);
        }
        else
        {
            stickers = JsonConvert.DeserializeObject<Sticker[]>(cachedStickers);
        }

        return stickers;
    }
}
