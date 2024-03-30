using Telegram.Bot;
using Telegram.Bot.Polling;
using Tgvs.Telegram;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        TelegramBotClientOptions options = new(builder.Configuration.GetValue<string>("TelegramBotToken"));
        return new TelegramBotClient(options, httpClient);
    });
builder.Services
    .AddHostedService<PollingService>()
    .AddSingleton<IReceiverService, ReceiverService>()
    .AddSingleton<IUpdateHandler, UpdateHandler>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
