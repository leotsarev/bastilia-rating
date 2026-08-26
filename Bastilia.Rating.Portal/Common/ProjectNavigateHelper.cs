using System.Security.Claims;
using Bastilia.Rating.Domain;
using Bastilia.Rating.Portal.Auth;
using Microsoft.AspNetCore.Components;

namespace Bastilia.Rating.Portal.Common
{
    public class ProjectNavigateHelper(IBastiliaProjectRepository projectRepository, NavigationManager navigationManager)
    {
        public async Task<BastiliaProjectWithDetails?> LoadProjectWithCheck(string projectIdOrSlug)
        {
            BastiliaProjectWithDetails? project;
            if (BastiliaProjectId.TryParse(projectIdOrSlug, null, out var projectId))
            {
                project = await projectRepository.GetByIdAsync(projectId);
            }
            else
            {
                project = await projectRepository.GetBySlugAsync(projectIdOrSlug);
            }

            if (project is null)
            {
                navigationManager.NavigateTo("/404");
                return null;
            }
            else
            {
                return project;
            }
        }

        public async Task<BastiliaProjectWithDetails?> LoadProjectForAdmin(string projectIdOrSlug, ClaimsPrincipal user)
        {
            var project = await LoadProjectWithCheck(projectIdOrSlug);
            if (project is null)
            {
                return null;
            }

            if (!user.IsProjectAdmin(project))
            {
                navigationManager.NavigateTo("/403");
                return null;
            }

            return project;
        }
    }
}
