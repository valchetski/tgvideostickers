using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Testcontainers.MsSql;
using Tgvs.Data;
using Tgvs.Tests.Integration.Fixtures;

namespace Tgvs.Tests.Integration;

public class BotApiTests
{
    [Fact]
    public async Task SqlProvider()
    {
        // arrange
        var container = new MsSqlBuilder().Build();
        await container.StartAsync();
        var sqlConnectionString = container.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__Stickers", sqlConnectionString);

        var mockTelegramBotClient = new MockTelegramBotClient();
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(x => x
                .ConfigureServices(s =>
                {
                    s.RemoveAll<ITelegramBotClient>();
                    s.AddSingleton<ITelegramBotClient>(mockTelegramBotClient);
                }));
        var client = factory.CreateClient();

        var stickers = new List<StickerEntity>();
        using (var scope = factory.Services.CreateScope())
        {
            var stickersDbContext = scope.ServiceProvider.GetRequiredService<StickersDbContext>();
            await stickersDbContext.Database.MigrateAsync();
            stickersDbContext.Add(new StickerEntity() { Title = "test", VideoFileId = "testvideo" });
            await stickersDbContext.SaveChangesAsync();
            stickers = [.. stickersDbContext.Stickers];
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
