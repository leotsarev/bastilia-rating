using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Bastilia.Rating.Tests;

internal static class TelegramTestFactory
{
    public static NewsHtmlRenderer CreateHtmlRenderer()
    {
        var services = new ServiceCollection();
        services.AddJoinHtmlSanitizers();
        return new NewsHtmlRenderer(services.BuildServiceProvider());
    }

    public static NewsMessageBuilder CreateMessageBuilder() => new(CreateHtmlRenderer());
}
