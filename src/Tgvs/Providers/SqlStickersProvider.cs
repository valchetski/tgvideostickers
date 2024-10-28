
using Microsoft.EntityFrameworkCore;
using Tgvs.Data;

namespace Tgvs.Providers;

public class SqlStickersProvider(StickersDbContext stickersContext) : IStickersProvider
{
    public async Task<Sticker[]?> GetStickersAsync()
    {
        return (await stickersContext.Stickers.ToArrayAsync()).Select(x => new Sticker
        {
            Id = x.Id.ToString(),
            Title = x.Title,
            VideoFileId = x.VideoFileId,
        }).ToArray();
    }
}
