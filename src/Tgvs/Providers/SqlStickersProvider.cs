
using Microsoft.EntityFrameworkCore;

namespace Tgvs;

public class SqlStickersProvider(StickersDbContext stickersContext) : IStickersProvider
{
    private readonly StickersDbContext _stickersContext = stickersContext;

    public async Task<Sticker[]?> GetStickersAsync()
    {
        return (await _stickersContext.Stickers.ToArrayAsync()).Select(x => new Sticker()
        {
            Id = x.Id.ToString(),
            Title = x.Title,
            VideoFileId = x.VideoFileId
        }).ToArray();
    }
}
