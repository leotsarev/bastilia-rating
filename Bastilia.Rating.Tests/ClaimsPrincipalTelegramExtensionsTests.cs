using System.Security.Claims;
using JoinRpg.Common.PrimitiveTypes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bastilia.Rating.Tests;

public class ClaimsPrincipalTelegramExtensionsTests
{
    [Fact]
    public void BothClaimsPresent_ReturnsLinkWithIdAndName()
    {
        var principal = PrincipalWithClaims(("telegram", "some_user"), ("telegram_chat_id", "987654321"));

        var telegram = Portal.Auth.ClaimsPrincipalTelegramExtensions.TryGetTelegram(principal, NullLogger.Instance);

        telegram.ShouldNotBeNull();
        telegram.Id.ShouldBe(987654321L);
        telegram.PrettyName?.Value.ShouldBe("some_user");
        telegram.IsVerified.ShouldBeTrue();
    }

    [Fact]
    public void ChatIdClaimOnly_ReturnsLinkWithoutName()
    {
        var principal = PrincipalWithClaims(("telegram_chat_id", "987654321"));

        var telegram = Portal.Auth.ClaimsPrincipalTelegramExtensions.TryGetTelegram(principal, NullLogger.Instance);

        telegram.ShouldNotBeNull();
        telegram.Id.ShouldBe(987654321L);
        telegram.PrettyName.ShouldBeNull();
        telegram.IsVerified.ShouldBeTrue();
    }

    [Fact]
    public void TelegramUsernameClaimOnly_ReturnsNull()
    {
        var principal = PrincipalWithClaims(("telegram", "some_user"));

        Portal.Auth.ClaimsPrincipalTelegramExtensions.TryGetTelegram(principal, NullLogger.Instance).ShouldBeNull();
    }

    [Fact]
    public void InvalidChatIdClaim_ReturnsNull()
    {
        var principal = PrincipalWithClaims(("telegram", "some_user"), ("telegram_chat_id", "not-a-number"));

        Portal.Auth.ClaimsPrincipalTelegramExtensions.TryGetTelegram(principal, NullLogger.Instance).ShouldBeNull();
    }

    [Fact]
    public void NoTelegramClaims_ReturnsNull()
    {
        var principal = PrincipalWithClaims(("preferred_username", "nick"));

        Portal.Auth.ClaimsPrincipalTelegramExtensions.TryGetTelegram(principal, NullLogger.Instance).ShouldBeNull();
    }

    private static ClaimsPrincipal PrincipalWithClaims(params (string Type, string Value)[] claims) =>
        new(new ClaimsIdentity(claims.Select(c => new Claim(c.Type, c.Value)).ToList()));
}
