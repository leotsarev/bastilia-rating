using Bastilia.Rating.Domain;
using Bastilia.Rating.Domain.DomainServices;
using JoinRpg.Common.PrimitiveTypes;
using Microsoft.AspNetCore.Components;

namespace Bastilia.Rating.Portal.Common
{
    public class UserLoaderHelper(IBastiliaMemberRepository bastiliaMemberRepository, NavigationManager navigationManager, UserImportService userImportService)
    {
        public async Task<BastiliaMember?> LoadUserWithCheck(string userIdOrSlug)
        {
            BastiliaMember? user;

            if (UserIdentification.TryParse(userIdOrSlug, null, out var userId))
            {
                user = await bastiliaMemberRepository.GetByIdAsync(userId);
                if (user is null)
                {
                    user = await userImportService.ImportUser(userId);
                }
            }
            else
            {
                user = await bastiliaMemberRepository.GetBySlugAsync(userIdOrSlug);
            }

            if (user is null)
            {
                navigationManager.NavigateTo("/404");
                return null;
            }
            else
            {
                return user;
            }
        }

        public async Task<List<BastiliaMember>> LoadUsersWithCheck(IReadOnlyList<string> userIdsOrSlugs)
        {
            var result = new List<BastiliaMember>();
            foreach (var userIdOrSlug in userIdsOrSlugs)
            {
                var user = await LoadUserWithCheck(userIdOrSlug);
                if (user != null)
                {
                    result.Add(user);
                }
            }

            return result;
        }
    }
}
