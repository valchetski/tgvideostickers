﻿using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Tgvs.Services;

namespace Tgvs.Telegram;

public class UpdateHandler(IStickersService stickersService, ILogger<UpdateHandler> logger)
    : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message } => BotOnMessageReceived(botClient, message, cancellationToken),
            { EditedMessage: { } message } => BotOnMessageReceived(botClient, message, cancellationToken),
            { InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(botClient, inlineQuery, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update),
        };

        await handler;
    }

    private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Receive message type: {MessageType}", message.Type);

        if (message.Video != null)
        {
            await TextInput(botClient, message, message.Video.FileId, cancellationToken);
            return;
        }

        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            _ => TextInput(botClient, message, "Hello", cancellationToken),
        };
        await action;
    }
    
    private static Task<Message> TextInput(ITelegramBotClient botClient, Message message, string text, CancellationToken cancellationToken)
    {
        return botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: text,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }
    
    private async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        var stickers = await stickersService.GetStickersAsync(inlineQuery.Query);
        InlineQueryResultCachedVideo[] results = [];
        if (stickers != null)
        {
            results = stickers
                .Select(x => new InlineQueryResultCachedVideo(x.Id, x.VideoFileId, x.Title))
                .ToArray();
        }
            
        await botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,
            cacheTime: 600,
            cancellationToken: cancellationToken);
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString(),
        };

        logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}

