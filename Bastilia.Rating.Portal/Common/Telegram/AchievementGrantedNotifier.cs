using Bastilia.Rating.Domain;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal class AchievementGrantedNotifier(
    IOptions<TelegramBotOptions> options,
    IOptions<JoinRpgHostNamesOptions> hostNames,
    ITelegramBotApi botApi,
    AchievementMessageBuilder builder,
    ILogger<AchievementGrantedNotifier> logger) : IAchievementGrantedNotifier
{
    public async Task NotifyAchievementGrantedAsync(BastiliaMember member, AchievementTemplate template, string projectName, string projectSlug, CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Telegram не настроен, личное уведомление о выдаче ачивки не отправлено");
            return;
        }

        // FromOptional с пустым chatId возвращает null, поэтому наличие Telegram
        // у члена клуба означает, что у него есть привязанный chat id.
        if (member.Telegram is not { } telegram || telegram.Id is not { } chatId)
        {
            logger.LogInformation("У пользователя {UserName} нет привязки Telegram, личное уведомление о выдаче ачивки не отправлено", member.UserName);
            return;
        }

        var message = builder.BuildMessage(member.UserName, template, projectName, projectSlug, hostNames.Value.RatingHost);

        try
        {
            await botApi.SendHtmlMessageAsync(new TelegramChatId(chatId), new TelegramHtmlString(message), cancellationToken);
        }
        catch (Exception exception)
        {
            var (retryable, errorMessage) = TelegramErrorClassifier.Classify(exception);
            logger.LogWarning(exception, "Не удалось отправить личное уведомление о выдаче ачивки в Telegram (повторяемая ошибка: {Retryable}): {ErrorMessage}", retryable, errorMessage);
        }
    }
}
