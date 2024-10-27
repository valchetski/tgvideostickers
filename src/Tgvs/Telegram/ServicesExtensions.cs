using Microsoft.AspNetCore.Http.Json;
using Telegram.Bot;

namespace Tgvs.Telegram;

public static class ServicesExtensions
{
    public static IServiceCollection AddTelegramServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .ConfigureTelegramBot<JsonOptions>(opt => opt.SerializerOptions);
        var telegramConfig = configuration.GetSection("Telegram").Get<TelegramBotConfig>();
        if (telegramConfig == null)
        {
            throw new InvalidOperationException("Missing Telegram configuration.");
        }
        
        var httpClientBuilder = services.AddHttpClient("telegram_bot_client");
        if (telegramConfig.UseMock)
        {
            var telegramBotClient = new MockTelegramBotClient();
            httpClientBuilder.AddTypedClient<ITelegramBotClient>((_, _) => telegramBotClient);
        }
        else
        {
            TelegramBotClientOptions options = new(telegramConfig.Token);
            httpClientBuilder
                .AddTypedClient<ITelegramBotClient>((httpClient) => new TelegramBotClient(options, httpClient));
        }   
        
        if (!telegramConfig.UseWebhook)
        {
            services.AddHostedService<PollingService>();
        }

        return services;
    }
}