using Tgvs.Providers;

namespace Tgvs.Services;

public interface IStickersService
{
    Task<Sticker[]?> GetStickersAsync(string name);
}
