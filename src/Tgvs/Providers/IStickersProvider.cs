namespace Tgvs.Providers;

public interface IStickersProvider
{
    public Task<Sticker[]?> GetStickersAsync();
}
