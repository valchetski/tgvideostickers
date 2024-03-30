using System.Text.Json;

namespace Tgvs;

public class FileStickersProvider(string fileName) : IStickersProvider
{
    public Sticker[] GetStickers()
    {
        return JsonSerializer.Deserialize<Sticker[]>(File.ReadAllText(fileName));
    }
}
