using System.Net;
using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Portal.Common.Telegram;

public class NewsMessageBuilder(NewsHtmlRenderer htmlRenderer)
{
    public string BuildMessage(ProjectNews news, string projectName, string? projectSlug, string ratingHost)
    {
        var header = $"<b>{WebUtility.HtmlEncode(projectName)}</b>" + (projectSlug is not null ? $" #{projectSlug}" : string.Empty);
        var body = htmlRenderer.RenderMarkdownToTelegramHtml(news.Text);
        var linkTarget = projectSlug ?? news.ProjectId.ToString();
        var footer = $"— {WebUtility.HtmlEncode(news.AuthorName)} · <a href=\"https://{ratingHost}/project/{linkTarget}\">{ratingHost}/project/{linkTarget}</a>";

        return string.Join("\n\n", header, body, footer);
    }
}
