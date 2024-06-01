using Microsoft.EntityFrameworkCore;

namespace Tgvs;

public class StickersDbContext(DbContextOptions<StickersDbContext> options) : DbContext(options)
{
    public DbSet<StickerEntity> Stickers{ get; set; }
}
