using Microsoft.EntityFrameworkCore;

namespace Tgvs.Data;

public class StickersDbContext(DbContextOptions<StickersDbContext> options) : DbContext(options)
{
    public DbSet<StickerEntity> Stickers { get; set; }
}
