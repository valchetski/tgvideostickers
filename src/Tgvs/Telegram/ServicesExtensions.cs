using Microsoft.AspNetCore.Http.Json;
using Telegram.Bot;

namespace Tgvs.Telegram;

public static class ServicesExtensions
{
    public static IServiceCollection AddTelegramServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .ConfigureTelegramBot<JsonOptions>(opt => opt.SerializerOptions);
        var telegramSection = configuration.GetSection("Telegram");

        var httpClientBuilder = services.AddHttpClient("telegram_bot_client");
        if (telegramSection.GetValue<bool>("UseMock"))
        {
            var telegramBotClient = new MockTelegramBotClient();
            httpClientBuilder.AddTypedClient<ITelegramBotClient>((_, _) => telegramBotClient);
        }
        else
        {
            var token = telegramSection.GetValue<string>("Token") ?? throw new InvalidOperationException("Telegram Token is not set.");
            TelegramBotClientOptions options = new(token);
            httpClientBuilder
                .AddTypedClient<ITelegramBotClient>((httpClient) => new TelegramBotClient(options, httpClient));
        }   
        
        if (!telegramSection.GetValue<bool>("UseWebhook"))
        {
            services.AddHostedService<PollingService>();
        }

        return services;
    }
}