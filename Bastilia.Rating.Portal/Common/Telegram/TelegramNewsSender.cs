using Bastilia.Rating.Domain;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal class TelegramNewsSender(
    IOptions<TelegramBotOptions> options,
    IOptions<JoinRpgHostNamesOptions> hostNames,
    ITelegramBotApi botApi,
    NewsMessageBuilder messageBuilder) : ITelegramNewsSender
{
    public async Task<NewsSendResult> Send(ProjectNews news, string projectName, string? projectSlug)
    {
        if (!options.Value.Enabled)
        {
            return NewsSendResult.Skipped("Telegram не настроен");
        }

        if (options.Value.ChatId == 0)
        {
            return NewsSendResult.Skipped("ChatId не настроен");
        }

        var message = messageBuilder.BuildMessage(news, projectName, projectSlug, hostNames.Value.RatingHost);

        try
        {
            await botApi.SendHtmlMessageAsync(new TelegramChatId(options.Value.ChatId), new TelegramHtmlString(message));
            return NewsSendResult.Success;
        }
        catch (Exception exception)
        {
            var (retryable, errorMessage) = TelegramErrorClassifier.Classify(exception);
            return NewsSendResult.Failure(errorMessage, retryable);
        }
    }
}
