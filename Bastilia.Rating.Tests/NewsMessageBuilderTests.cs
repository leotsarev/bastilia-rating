using Bastilia.Rating.Portal.Common.Telegram;

namespace Bastilia.Rating.Tests;

public class NewsMessageBuilderTests
{
    private static ProjectNews CreateNews(int projectId = 2, string text = "**текст**") =>
        new(1, projectId, 3, "Имя Автора", DateTimeOffset.UtcNow, text, NewsSendStatus.NotSent, null);

    [Fact]
    public void WithSlug_BuildsHeaderHashtagBodyFooter()
    {
        var message = TelegramTestFactory.CreateMessageBuilder().BuildMessage(CreateNews(), "Мой проект", "my-project", "rating.bastilia.ru");

        message.ShouldStartWith("<b>Мой проект</b> #my-project");
        message.ShouldContain("<strong>текст</strong>");
        message.ShouldEndWith("— Имя Автора · <a href=\"https://rating.bastilia.ru/project/my-project\">rating.bastilia.ru/project/my-project</a>");
    }

    [Fact]
    public void WithoutSlug_UsesProjectIdInLinkAndOmitsHashtag()
    {
        var message = TelegramTestFactory.CreateMessageBuilder().BuildMessage(CreateNews(), "Мой проект", null, "rating.bastilia.ru");

        message.ShouldStartWith("<b>Мой проект</b>");
        message.ShouldNotContain("#");
        message.ShouldContain("https://rating.bastilia.ru/project/2");
    }

    [Fact]
    public void ProjectNameWithHtml_IsEscaped()
    {
        var message = TelegramTestFactory.CreateMessageBuilder().BuildMessage(CreateNews(), "<b>x</b>", "my-project", "rating.bastilia.ru");

        message.ShouldContain("&lt;b&gt;x&lt;/b&gt;");
    }
}
