using System.Text.RegularExpressions;
using Bastilia.Rating.Portal.Common.Telegram;

namespace Bastilia.Rating.Tests;

public class NewsHtmlRendererTests
{
    private readonly NewsHtmlRenderer renderer = TelegramTestFactory.CreateHtmlRenderer();

    [Fact]
    public void BoldMarkdown_BecomesStrong()
    {
        var result = renderer.RenderMarkdownToTelegramHtml("**жирный**");

        result.ShouldContain("<strong>жирный</strong>");
    }

    [Fact]
    public void LinkMarkdown_BecomesAnchorWithHref()
    {
        var result = renderer.RenderMarkdownToTelegramHtml("[текст](https://example.com)");

        result.ShouldContain("<a href=\"https://example.com/\" target=\"_blank\" rel=\"nofollow\">текст</a>");
    }

    [Fact]
    public void ScriptTag_IsStripped()
    {
        var result = renderer.RenderMarkdownToTelegramHtml("<script>alert(1)</script>привет");

        result.ShouldNotContain("<script");
        result.ShouldNotContain("</script");
        result.ShouldContain("привет");
    }

    [Fact]
    public void JavascriptLink_HrefIsRemoved()
    {
        var result = renderer.RenderMarkdownToTelegramHtml("[клик](javascript:alert(1))");

        result.ShouldNotContain("javascript:");
        result.ShouldContain("клик");
    }

    [Fact]
    public void MailtoLink_HrefPassesThrough()
    {
        var result = renderer.RenderMarkdownToTelegramHtml("[почта](mailto:test@example.com)");

        result.ShouldContain("<a href=\"mailto:test@example.com\" target=\"_blank\" rel=\"nofollow\">почта</a>");
    }

    [Fact]
    public void TwoParagraphs_SeparatedByNewline()
    {
        var result = renderer.RenderMarkdownToTelegramHtml("один\n\nдва");

        Regex.IsMatch(result, "один\n+два").ShouldBeTrue();
    }

    [Fact]
    public void ListItems_SeparatedByNewline()
    {
        var result = renderer.RenderMarkdownToTelegramHtml("- один\n- два");

        result.ShouldContain("один");
        result.ShouldContain("два");
        Regex.IsMatch(result, "один\n+два").ShouldBeTrue();
    }
}
