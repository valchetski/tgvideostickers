using Microsoft.Extensions.Caching.Memory;

namespace Tgvs;

public static class ServicesExtensions
{
    public static IServiceCollection AddStickersProviders(this IServiceCollection services, IConfiguration configuration)
    {
        var stickersFile = configuration.GetValue<string>("StickersFile");

        IStickersProvider originalProvider;

        if (Uri.IsWellFormedUriString(stickersFile, UriKind.Absolute))
        {
            originalProvider = new RemoteStickersProvider(stickersFile);
        }
        else
        {
            originalProvider = new FileStickersProvider(stickersFile);
        }

        services.AddSingleton<IStickersProvider>(sp => 
            new CachedStickersProvider(originalProvider, sp.GetRequiredService<IMemoryCache>()));
        
        return services;
    }
}
