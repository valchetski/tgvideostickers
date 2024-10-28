namespace Tgvs.Telegram;

public class TelegramBotConfig
{
    public required string Token { get; init; }

    public bool UseWebhook { get; init; }
    
    public bool UseMock { get; init; }
}
