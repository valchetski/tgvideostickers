using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Requests.Abstractions;

namespace Tgvs.Tests.Integration.Fixtures;

public class MockTelegramBotClient : ITelegramBotClient
{
    public List<AnswerInlineQueryRequest> AnswerInlineQueryRequests { get; set; } = [];

    public bool LocalBotServer => throw new NotImplementedException();

    public long? BotId => throw new NotImplementedException();

    public TimeSpan Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IExceptionParser ExceptionsParser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event AsyncEventHandler<ApiRequestEventArgs>? OnMakingApiRequest;
    public event AsyncEventHandler<ApiResponseEventArgs>? OnApiResponseReceived;

    public Task DownloadFileAsync(string filePath, Stream destination, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<TResponse> MakeRequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestArgs = new ApiRequestEventArgs(request);

        if (OnMakingApiRequest != null)
        {
            await OnMakingApiRequest.Invoke(this, requestArgs, cancellationToken);
        }

        if (request is AnswerInlineQueryRequest answerInlineQueryRequest)
        {
            AnswerInlineQueryRequests.Add(answerInlineQueryRequest);
        }

        if (OnApiResponseReceived != null)
        {
            await OnApiResponseReceived.Invoke(this, new ApiResponseEventArgs(new HttpResponseMessage(), requestArgs), cancellationToken);
        }
        return Activator.CreateInstance<TResponse>();
    }

    public Task<bool> TestApiAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
