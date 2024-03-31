using System.Text.Json;

namespace Tgvs;

public class FileStickersProvider(string fileName) : IStickersProvider
{
    public Sticker[] GetStickers(string name)
    {
        var stickers = JsonSerializer.Deserialize<Sticker[]>(File.ReadAllText(fileName));
        if (string.IsNullOrEmpty(name))
        {
            return stickers;
        }

        return stickers.Where(x => x.Title.Contains(name, StringComparison.OrdinalIgnoreCase)).ToArray();
    }
}
