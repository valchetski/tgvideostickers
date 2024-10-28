using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Testcontainers.MsSql;
using Tgvs.Data;
using Tgvs.Telegram;

namespace Tgvs.Tests.Integration;

public class BotApiTests
{
    [Fact]
    public async Task SqlProvider()
    {
        // arrange
        var container = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        var sqlConnectionString = container.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__Stickers", sqlConnectionString);
        Environment.SetEnvironmentVariable("Telegram__UseMock", "true");

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(x => x
                .ConfigureServices(services =>
                {
                    services.RemoveAll<IDistributedCache>();
                    
                    var distributedCacheMock = new Mock<IDistributedCache>();
                    services.AddSingleton(distributedCacheMock.Object);
                }));
        var client = factory.CreateClient();

        var stickers = new List<StickerEntity> { new() { Title = "test", VideoFileId = "video" } };
        using (var scope = factory.Services.CreateScope())
        {
            var stickersDbContext = scope.ServiceProvider.GetRequiredService<StickersDbContext>();
            await stickersDbContext.Database.MigrateAsync();
            stickersDbContext.AddRange(stickers);
            await stickersDbContext.SaveChangesAsync();
        }

        var update = new Update
        {
            InlineQuery = new InlineQuery
            {
                Id = "any",
                From = new User()
                {
                    FirstName = string.Empty,
                },
                Query = string.Empty,
                Offset = string.Empty,
            }
        };
        var stringPayload = JsonSerializer.Serialize(update, JsonBotAPI.Options);
        var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

        // act 
        var response = await client.PostAsync("/bot", httpContent);
        var content = await response.Content.ReadAsStringAsync();

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        var mockTelegramBotClient = factory.Services
            .GetRequiredService<ITelegramBotClient>().Should().BeOfType<MockTelegramBotClient>().Which;

        mockTelegramBotClient.AnswerInlineQueryRequests.Should().NotBeNullOrEmpty()
            .And.ContainSingle(x => x.InlineQueryId == update.InlineQuery.Id)
            .Which.Results.Should().NotBeNullOrEmpty()
            .And.AllSatisfy(x =>
            {
                var stickerSeed = stickers.Should().ContainSingle(s => s.Id.ToString() == x.Id).Which;
                var stickerDb = x.Should().BeOfType<InlineQueryResultCachedVideo>().Which;
                stickerDb.Title.Should().Be(stickerSeed.Title);
                stickerDb.VideoFileId.Should().Be(stickerSeed.VideoFileId);
            });

        // second response should be the same and use cache 
        response = await client.PostAsync("/bot", httpContent);
        content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
    }
}
