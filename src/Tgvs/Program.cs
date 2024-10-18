using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Tgvs.Cache;
using Tgvs.Providers;
using Tgvs.Services;
using Tgvs.Telegram;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHttpLogging(o => { })
    .AddTgvsSqliteCache()
    .AddControllers();
    
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

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

if (!telegramSection.GetValue<bool>("UseWebhook"))
{
    builder.Services.AddHostedService<PollingService>();
}

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.MapControllers();

await app.RunAsync();
