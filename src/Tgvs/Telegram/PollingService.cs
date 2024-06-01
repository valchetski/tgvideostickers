using Telegram.Bot;

namespace Tgvs.Telegram;

public class PollingService(
    IServiceProvider serviceProvider,
    ITelegramBotClient botClient) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await botClient.DeleteWebhookAsync(false, cancellationToken);
        using var scope = serviceProvider.CreateScope();
        var receiverService = scope.ServiceProvider.GetRequiredService<IReceiverService>();
        receiverService.Receive(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
