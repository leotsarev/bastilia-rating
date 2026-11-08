using JoinRpg.Common.WebInfrastructure.HtmlSanitize;
using Markdig;
using Microsoft.Extensions.DependencyInjection;

namespace Bastilia.Rating.Portal.Common.Telegram;

public class NewsHtmlRenderer(IServiceProvider serviceProvider)
{
    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseAutoLinks()
        .DisableHtml()
        .Build();

    public string RenderMarkdownToTelegramHtml(string markdown)
    {
        var html = Markdown.ToHtml(markdown, MarkdownPipeline);

        // IJoinTelegramHtmlSanitizer — одноразовый: Dispose возвращает экземпляр в пул,
        // поэтому берём новый из DI на каждый рендер.
        using var sanitizer = serviceProvider.GetRequiredService<IJoinTelegramHtmlSanitizer>();
        return sanitizer.Sanitize(ToTelegramHtmlString(html));
    }

    /// <summary>
    /// Эквивалент ToTelegramHtmlString из JoinRpg.Markdown (MarkDownRendererFacade):
    /// Telegram не понимает блочные теги, а telegram-санитайзер (IJoinTelegramHtmlSanitizer)
    /// выбрасывает их содержимое-теги (p, ul, ol, li), поэтому переносы строк вставляем заранее,
    /// пока закрывающие теги ещё на месте.
    /// </summary>
    /// <remarks>
    /// Отличия от joinrpg: там заменяется только <c>&lt;/p&gt;</c> (плюс TrimEnd) и финальная
    /// telegram-санитизация с флэттингом происходит отдельным этапом при отправке; у нас обёртки
    /// списков (ul/ol) убираются здесь же, а пункты (li) разделяются переносом, чтобы после
    /// флэттинга пункты и абзацы не слипались.
    /// </remarks>
    private static string ToTelegramHtmlString(string html)
        => html
            .Replace("</p>", "</p>\n")
            .Replace("</li>", "</li>\n")
            .Replace("<ul>", string.Empty)
            .Replace("</ul>", string.Empty)
            .Replace("<ol>", string.Empty)
            .Replace("</ol>", string.Empty)
            .Trim();
}
