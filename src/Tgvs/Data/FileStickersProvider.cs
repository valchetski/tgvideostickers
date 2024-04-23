using System.Text.Json;

namespace Tgvs;

public class FileStickersProvider(string fileName) : IStickersProvider
{
    public Task<Sticker[]?> GetStickersAsync()
    {
        return Task.FromResult(JsonSerializer.Deserialize<Sticker[]>(File.ReadAllText(fileName)));
    }
}
