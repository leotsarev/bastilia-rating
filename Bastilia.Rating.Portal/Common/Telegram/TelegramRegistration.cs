using Bastilia.Rating.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bastilia.Rating.Portal.Common.Telegram;

public static class TelegramRegistration
{
    public static IServiceCollection AddTelegramNewsSender(this IServiceCollection services)
    {
        services.AddOptions<TelegramBotOptions>().BindConfiguration("Telegram");
        services.AddSingleton<ITelegramBotApi, TelegramBotApi>();
        services.AddTransient<ITelegramNewsSender, TelegramNewsSender>();
        services.AddTransient<NewsMessageBuilder>();
        services.AddTransient<NewsHtmlRenderer>();

        // Telegram — опциональная интеграция, поэтому без тега "ready": выключенный
        // Telegram не должен задерживать /health/ready.
        services.AddHealthChecks().AddCheck<TelegramHealthCheck>("Telegram client");

        return services;
    }
}
