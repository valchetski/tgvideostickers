using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Tgvs.Data;

namespace Tgvs.Providers;

public static class ServicesExtensions
{
    public static IServiceCollection AddCachedStickersProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var stickersConnectionString = configuration.GetConnectionString("Stickers");
        if (!string.IsNullOrEmpty(stickersConnectionString))
        {
            return services
                .AddDbContext<StickersDbContext>(options => options
                    .UseSqlServer(stickersConnectionString, builder =>
                        builder.EnableRetryOnFailure()))
                .AddCachedStickersProvider(sp =>
                    new SqlStickersProvider(sp.GetRequiredService<StickersDbContext>()));
        }

        var stickersFile = configuration.GetValue<string>("StickersFile");
        ArgumentNullException.ThrowIfNull(stickersFile);
        return Uri.IsWellFormedUriString(stickersFile, UriKind.Absolute) ? 
            services.AddCachedStickersProvider(new RemoteStickersProvider(stickersFile)) : 
            services.AddCachedStickersProvider(new FileStickersProvider(stickersFile));
    }

    private static IServiceCollection AddCachedStickersProvider<TProvider>(
        this IServiceCollection services,
        Func<IServiceProvider, TProvider> getProvider) where TProvider : IStickersProvider
    {
        return services.AddScoped<IStickersProvider>(sp =>
            new CachedStickersProvider(
                getProvider(sp),
                sp.GetRequiredService<IDistributedCache>(),
                sp.GetRequiredService<IOptions<DistributedCacheEntryOptions>>()));
    }

    private static IServiceCollection AddCachedStickersProvider(
        this IServiceCollection services,
        IStickersProvider provider)
    {
        return services.AddSingleton<IStickersProvider>(sp =>
            new CachedStickersProvider(
                provider,
                sp.GetRequiredService<IDistributedCache>(),
                sp.GetRequiredService<IOptions<DistributedCacheEntryOptions>>()));
    }
}
