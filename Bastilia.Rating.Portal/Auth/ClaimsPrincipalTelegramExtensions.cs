using System.Globalization;
using System.Security.Claims;
using JoinRpg.Common.PrimitiveTypes;
using Microsoft.Extensions.Logging;

namespace Bastilia.Rating.Portal.Auth;

internal static class ClaimsPrincipalTelegramExtensions
{
    public static TelegramSocialLink? TryGetTelegram(this ClaimsPrincipal principal, ILogger logger)
    {
        var telegramUsername = principal.FindFirst("telegram")?.Value;
        var telegramChatIdClaim = principal.FindFirst("telegram_chat_id")?.Value;
        long? telegramChatId = null;
        if (telegramChatIdClaim is not null)
        {
            if (long.TryParse(telegramChatIdClaim, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedChatId))
            {
                telegramChatId = parsedChatId;
            }
            else
            {
                logger.LogWarning("Не удалось распарсить claim telegram_chat_id '{TelegramChatIdClaim}'; привязка Telegram по этому claim обновлена не будет",
                    telegramChatIdClaim);
            }
        }

        // Chat id приходит только из верифицированной привязки Telegram (ExternalLogin на id.joinrpg.ru).
        var telegram = TelegramSocialLink.FromOptional(telegramChatId, PrefferedName.FromOptional(telegramUsername), isVerified: true);
        if (telegram is null && telegramUsername is not null)
        {
            logger.LogDebug("Claim telegram есть ('{TelegramUsername}'), но telegram_chat_id отсутствует — привязка Telegram не обновляется",
                telegramUsername);
        }
        return telegram;
    }
}
