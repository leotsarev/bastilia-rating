namespace Bastilia.Rating.Database;

internal class BastiliaProjectRepository(IDbContextFactory<AppDbContext> contextFactory) : BastiliaRepositoryBase, IBastiliaProjectRepository
{
    public Task<BastiliaProjectWithDetails?> GetByIdAsync(BastiliaProjectId projectId) => GetOneProjectByPredicate(p => p.BastiliaProjectId == projectId);

    public Task<BastiliaProjectWithDetails?> GetBySlugAsync(string slug) => GetOneProjectByPredicate(p => p.Slug == slug);


    public Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects() => GetProjectsByPredicate(p => p.EndDate == null);

    public Task<IReadOnlyCollection<BastiliaProject>> GetAllProjects() => GetProjectsByPredicate(p => true);

    public async Task<IReadOnlyCollection<BastiliaProjectId>> GetProjectIdsForCoordinator(UserIdentification joinrpgUserId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return [.. await context.ProjectAdmins
            .Where(pa => pa.UserId == joinrpgUserId && pa.RemoveDate == null)
            .Select(pa => pa.ProjectId)
            .ToListAsync()];
    }

    public async Task<IReadOnlyCollection<BastiliaCalendarItem>> GetProjectCalendarFor(int year)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return [..Query(context)
            .Where(x => x.PlannedEndDate!.Value.Year == year || x.PlannedStartDate.Year == year)
            .Where(x => !x.OngoingProject)
            .Where(x => x.DeletedAt == null)
            .Select(x =>
            new BastiliaCalendarItem(BastiliaCalendarItemType.Project, x.StartDate ?? x.PlannedStartDate, x.EndDate ?? x.PlannedEndDate ?? x.PlannedStartDate, x.ProjectName, x.BastiliaProjectId))];
    }

    private async Task<IReadOnlyCollection<BastiliaProject>> GetProjectsByPredicate(Expression<Func<Entities.BastiliaProject, bool>> predicate)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var projects = await Query(context)
                    .Where(predicate)
                    .Select(p => new
                    {
                        Project = p,
                        TemplatesCount = p.AchievementTemplates.Count,
                        AchievementsCount = p.AchievementTemplates.SelectMany(t => t.Achievements).Count()
                    })
                    .ToListAsync();

        return [.. projects.Select(x => ToProject(x.Project, x.TemplatesCount, x.AchievementsCount))];
    }

    private static IQueryable<Entities.BastiliaProject> Query(AppDbContext context)
    {
        return context.BastiliaProjects
                    .Include(bp => bp.ProjectAdmins)
                    .ThenInclude(pa => pa.User)
                    .AsNoTracking()
                    ;
    }


    private async Task<BastiliaProjectWithDetails?> GetOneProjectByPredicate(Expression<Func<Entities.BastiliaProject, bool>> predicate)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var project = await Query(context)
                    .Where(predicate)
                    .Include(p => p.AchievementTemplates)
                    .ThenInclude(p => p.Achievements)
                    .ThenInclude(p => p.User)
                    .Include(p => p.AchievementTemplates).ThenInclude(at => at.Achievements).ThenInclude(a => a.GrantedByUser)
                    .Include(p => p.AchievementTemplates).ThenInclude(at => at.Achievements).ThenInclude(a => a.RemovedByUser)
                    .FirstOrDefaultAsync();

        if (project == null) { return null; }


        return new BastiliaProjectWithDetails(
           project.BastiliaProjectId,
           project.ProjectName,
           project.ProjectType,
           project.BrandType,
           project.OngoingProject,
           project.ProjectOfTheYear,
           project.JoinrpgProjectId,
           project.KogdaIgraProjectId,
           project.ProjectUri,
           [.. project.ProjectAdmins.Where(pa => pa.RemoveDate == null).Select(pa => ToUserLink(pa.User))],
           [.. project.AchievementTemplates.SelectMany(a => a.Achievements).Select(ToMemberAchievement)],
           [.. project.AchievementTemplates.Select(ToTemplate)],
           project.EndDate,
           project.HowToHelp,
           project.ProjectDescription,
           new Uri(project.ProjectIconUri),
           project.Slug,
           project.DeletedAt,
           project.LastUpdatedAt,
           project.PlannedEndDate
           );
    }
}
