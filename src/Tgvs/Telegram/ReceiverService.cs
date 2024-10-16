using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Tgvs.Telegram;

public class ReceiverService(
    ITelegramBotClient botClient,
    IUpdateHandler updateHandler,
    ILogger<ReceiverService> logger) : IReceiverService
{
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IUpdateHandler _updateHandler = updateHandler;
    private readonly ILogger<ReceiverService> _logger = logger;

    public void Receive(CancellationToken stoppingToken)
    {
        // ToDo: we can inject ReceiverOptions through IOptions container
        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = [],
            DropPendingUpdates = true,
        };

        _logger.LogInformation("Start receiving updates");

        _botClient.StartReceiving(
            updateHandler: _updateHandler,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }
}
