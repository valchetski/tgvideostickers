namespace Tgvs;

public interface IStickersProvider
{
    public Task<Sticker[]?> GetStickersAsync();
}
