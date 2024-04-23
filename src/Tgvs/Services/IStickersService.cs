namespace Tgvs;

public interface IStickersService
{
    Task<Sticker[]?> GetStickersAsync(string name);
}
