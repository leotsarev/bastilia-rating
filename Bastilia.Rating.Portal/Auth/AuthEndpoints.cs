using Bastilia.Rating.Domain;
using Bastilia.Rating.Domain.DomainServices;
using JoinRpg.Common.PrimitiveTypes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Client.AspNetCore;
using System.Security.Claims;

namespace Bastilia.Rating.Portal.Auth;

internal static class AuthEndpoints
{
    internal static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/login", (HttpContext context) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/signin-joinrpg",
            };
            return Results.Challenge(properties, [OpenIddictClientAspNetCoreDefaults.AuthenticationScheme]);
        });

        endpoints.MapGet("/signin-joinrpg", async (HttpContext context, UserImportService userImportService, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Auth");

            var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                logger.LogWarning("OIDC authentication failed: {Error}", result.Failure?.Message);
                return Results.Problem(
                    detail: result.Failure?.Message ?? "Authentication callback failed",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var sub = result.Principal?.FindFirstValue("sub");
            if (sub is null || !UserIdentification.TryParse(sub, provider: null, out var userId))
            {
                logger.LogWarning("Failed to extract user ID from claims. sub={Sub}, claims=[{Claims}]",
                    sub, string.Join(", ", result.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? []));
                return Results.Problem(
                    detail: $"Could not extract numeric user ID from 'sub' claim (got: {sub})",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var member = await userImportService.ImportUser(userId);

            if (member is null)
            {
                logger.LogWarning("Не удалось загрузить пользователя {userId} с id.joinrpg.ru", userId);
                return Results.Problem(
                    detail: $"Не удалось загрузить пользователя {userId} с id.joinrpg.ru",
                    statusCode: StatusCodes.Status401Unauthorized);

            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, member.JoinrpgUserId.Value.ToString()),
                new(ClaimTypes.Name, member.UserName),
                new("avatar", member.AvatarUrl)
            };

            if (member.IsActiveMember)
            {
                claims.Add(new(ClaimTypes.Role, BastiliaRoles.Member));
            }
            if (member.IsPresident)
            {
                claims.Add(new(ClaimTypes.Role, BastiliaRoles.President));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return Results.Redirect("/");
        });

        endpoints.MapGet("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });
    }
}
