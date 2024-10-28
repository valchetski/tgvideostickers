using Tgvs.Providers;

namespace Tgvs.Services;

public class StickersService(IStickersProvider stickersProvider) : IStickersService
{
    public async Task<Sticker[]?> GetStickersAsync(string name)
    {
        var stickers = await stickersProvider.GetStickersAsync();

        if (stickers == null)
        {
            return null;
        }

        return string.IsNullOrEmpty(name) 
            ? stickers 
            : stickers.Where(x => x.Title.Contains(name, StringComparison.OrdinalIgnoreCase)).ToArray();
    }
}
