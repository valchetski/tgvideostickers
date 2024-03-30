namespace Tgvs.Telegram;

public class PollingService(IReceiverService receiverService) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        receiverService.Receive(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
