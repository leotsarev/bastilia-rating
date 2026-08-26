using System.Security.Claims;
using Bastilia.Rating.Domain;
using JoinRpg.Common.PrimitiveTypes;

namespace Bastilia.Rating.Portal.Auth;

internal static class ClaimsPrincipalExtensions
{
    public static UserIdentification GetJoinrpgUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return value is not null && UserIdentification.TryParse(value, null, out var id) ? id : throw new InvalidOperationException("User has no valid JoinrpgUserId claim");
    }

    public static bool IsProjectAdmin(this ClaimsPrincipal user, BastiliaProject project)
    {
        return user.IsInRole(BastiliaRoles.President)
            || project.Coordinators.Any(c => c.JoinrpgUserId == user.GetJoinrpgUserId());
    }
}
