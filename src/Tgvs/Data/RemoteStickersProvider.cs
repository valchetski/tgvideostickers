namespace Tgvs;

public class RemoteStickersProvider(string remoteFileUrl) : IStickersProvider
{
    private readonly HttpClient _httpClient = new();

    public async Task<Sticker[]?> GetStickersAsync()
    {
        var response = await _httpClient.GetAsync(remoteFileUrl);
        var content = await response.Content.ReadFromJsonAsync<Sticker[]>();
        return content;
    }
}
