using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Tgvs;
using Tgvs.Telegram;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddNewtonsoftJson();
    
builder.Services
    .AddHttpLogging(o => { })
    .AddMemoryCache()
    .AddApplicationInsightsTelemetry();

var telegramSection = builder.Configuration.GetSection("Telegram");
builder.Services.Configure<TelegramBotConfig>(telegramSection);
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var telegramConfig = sp.GetRequiredService<IOptions<TelegramBotConfig>>().Value;
        TelegramBotClientOptions options = new(telegramConfig.Token);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services    
    .AddCachedStickersProvider(builder.Configuration)
    .AddScoped<IStickersService, StickersService>()
    .AddScoped<IReceiverService, ReceiverService>()
    .AddScoped<IUpdateHandler, UpdateHandler>();

if (telegramSection.GetValue<bool>("UseWebhook"))
{
    builder.Services.AddHostedService<ConfigureWebhook>();
}
else
{
    builder.Services.AddHostedService<PollingService>();
}

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.MapControllers();

app.Run();
