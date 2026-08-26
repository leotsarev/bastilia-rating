namespace Bastilia.Rating.Domain;

public interface IBastiliaMemberRepository
{
    Task<BastiliaMember?> GetByIdAsync(UserIdentification userId);
    Task<BastiliaMember?> GetBySlugAsync(string slug);

    Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync();

    Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync();

    Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory();
    Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year);
}

public interface IBastiliaProjectRepository
{
    Task<BastiliaProjectWithDetails?> GetByIdAsync(BastiliaProjectId projectId);
    Task<BastiliaProjectWithDetails?> GetBySlugAsync(string slug);
    Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects();
    Task<IReadOnlyCollection<BastiliaProject>> GetAllProjects();
    Task<IReadOnlyCollection<BastiliaCalendarItem>> GetProjectCalendarFor(int year);
    Task<IReadOnlyCollection<BastiliaProjectId>> GetProjectIdsForCoordinator(UserIdentification joinrpgUserId);
}

public interface IBastiliaTemplateRepository
{
    Task<IReadOnlyCollection<AchievementTemplate>> GetAchievementTemplates();
}

public interface IBastiliaKograIgraRepository
{
    Task<IReadOnlyCollection<BastiliaCalendarItem>> GetGameCalendarFor(int year);
}
