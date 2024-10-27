using Microsoft.AspNetCore.Mvc;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Tgvs.Cache;
using Tgvs.Providers;
using Tgvs.Services;
using Tgvs.Telegram;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHttpLogging(_ => { })
    .AddTgvsSqliteCache()
    .AddTelegramServices(builder.Configuration)
    .AddCachedStickersProvider(builder.Configuration)
    .AddScoped<IStickersService, StickersService>()
    .AddScoped<IReceiverService, ReceiverService>()
    .AddScoped<IUpdateHandler, UpdateHandler>();
    
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");
app.MapPost("/bot", async (
    [FromBody] Update update,
    CancellationToken cancellationToken,
    IUpdateHandler updateHandler,
    ITelegramBotClient botClient) =>
{
    await updateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
    return Results.Ok();
});

await app.RunAsync();
