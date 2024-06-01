
using Microsoft.Extensions.Caching.Memory;

namespace Tgvs;

public class CachedStickersProvider(IStickersProvider originalProvider, IMemoryCache memoryCache)
     : IStickersProvider
{
    private readonly IStickersProvider _originalProvider = originalProvider;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<Sticker[]?> GetStickersAsync()
    {
        var stickers = _memoryCache.Get<Sticker[]>("Stickers");
        if (stickers == null)
        {
            stickers = await _originalProvider.GetStickersAsync();
            _memoryCache.Set("Stickers", stickers, DateTimeOffset.UtcNow.AddMinutes(5));
        }

        return stickers;
    }
}
