namespace Tgvs.Telegram;

public interface IReceiverService
{
    void Receive(CancellationToken stoppingToken);
}
