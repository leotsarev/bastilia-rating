using Bastilia.Rating.Portal.Common.Telegram;
using VerifyXunit;

namespace Bastilia.Rating.Tests;

public class NewsMessageSnapshotTests : VerifyBase
{
    public NewsMessageSnapshotTests() : base()
    {
    }

    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private const string Markdown = """
        **Важная новость!**

        Первый абзац с [ссылкой](https://example.com).

        Второй абзац со *курсивом*.
        """;

    [Fact]
    public Task BuildMessage_ProducesStableTelegramPost()
    {
        var news = new ProjectNews(1, 2, 3, "Имя Автора", CreatedAt, Markdown, NewsSendStatus.NotSent, null);

        var message = TelegramTestFactory.CreateMessageBuilder().BuildMessage(news, "Мой проект", "my-project", "rating.bastilia.ru");

        return Verify(message);
    }

    [Fact]
    public Task RenderMarkdownToTelegramHtml_ProducesStableOutput()
    {
        var html = TelegramTestFactory.CreateHtmlRenderer().RenderMarkdownToTelegramHtml(Markdown);

        return Verify(html);
    }
}
