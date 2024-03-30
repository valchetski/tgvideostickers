using Telegram.Bot;

namespace Tgvs.Telegram;

public class PollingService(
    IReceiverService receiverService,
    ITelegramBotClient botClient) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await botClient.DeleteWebhookAsync(false, cancellationToken);
        receiverService.Receive(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
