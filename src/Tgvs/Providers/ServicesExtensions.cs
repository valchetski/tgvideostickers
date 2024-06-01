using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Tgvs;

public static class ServicesExtensions
{
    public static IServiceCollection AddCachedStickersProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var stickersConnectionString = configuration.GetConnectionString("Stickers");
        if (!string.IsNullOrEmpty(stickersConnectionString))
        {
            return services
                .AddDbContext<StickersDbContext>(options => options
                    .UseSqlServer(stickersConnectionString))
                .AddCachedStickersProvider(sp => 
                    new SqlStickersProvider(sp.GetRequiredService<StickersDbContext>()));
        }
        else
        {
            var stickersFile = configuration.GetValue<string>("StickersFile");
            ArgumentNullException.ThrowIfNull(stickersFile);
            if (Uri.IsWellFormedUriString(stickersFile, UriKind.Absolute))
            {
                return services.AddCachedStickersProvider(new RemoteStickersProvider(stickersFile));
            }
            else
            {
                return services.AddCachedStickersProvider(new FileStickersProvider(stickersFile));
            }
        }
    }

    private static IServiceCollection AddCachedStickersProvider<TProvider>(
        this IServiceCollection services,
        Func<IServiceProvider, TProvider> getProvider) where TProvider : IStickersProvider
    {
        return services.AddScoped<IStickersProvider>(sp => 
            new CachedStickersProvider(getProvider(sp), sp.GetRequiredService<IMemoryCache>()));
    }

    private static IServiceCollection AddCachedStickersProvider(
        this IServiceCollection services,
        IStickersProvider provider)
    {
        return services.AddSingleton<IStickersProvider>(sp => 
            new CachedStickersProvider(provider, sp.GetRequiredService<IMemoryCache>()));
    }
}
