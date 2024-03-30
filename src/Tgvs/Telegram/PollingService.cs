using Telegram.Bot;

namespace Tgvs.Telegram;

public class PollingService(
    IReceiverService receiverService,
    ITelegramBotClient botClient) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        botClient.DeleteWebhookAsync(false, cancellationToken);
        receiverService.Receive(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
