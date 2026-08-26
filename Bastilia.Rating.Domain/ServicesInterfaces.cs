namespace Bastilia.Rating.Domain
{
    public interface IAchievementService
    {
        Task GrantAchivement(BastiliaProjectId projectId, UserIdentification userId, TemplateId templateId, UserIdentification grantedById, string? overrideName);
    }

    public interface IUserDbService
    {
        Task<BastiliaMember> AddUser(UserIdentification playerId, string nickName, string avatarUrl);
    }

    public interface IKiDbService
    {
        Task AddKogdaIgraGame(int kogdaIgraId, string name, DateOnly begin, DateOnly end, DateTimeOffset lastUpdatedAt);
    }

    public interface IProjectService
    {
        Task<BastiliaProjectId> CreateProject(string projectName, ProjectType projectType, BrandType brandType, bool OngoingProject, int? JoinrpgProjectId, int? KogdaIgraProjectId, string ProjectUri, IReadOnlyList<UserIdentification> coordinators,
            DateOnly startDate, DateOnly endDate, bool alreadyCompleted, string projectDescription);

        Task CompleteProject(BastiliaProjectId projectId, DateOnly endDate, ProjectLevel projectLevel, IReadOnlyList<AchievementTemplateInput> achievementTemplates);

        Task AddAchievementTemplates(BastiliaProjectId projectId, ProjectLevel projectLevel, IReadOnlyList<AchievementTemplateInput> achievementTemplates);

        Task UpdateCoordinators(BastiliaProjectId projectId, IReadOnlyList<UserIdentification> coordinators);

        Task UpdateAchievementTemplate(TemplateId templateId, string name, string description);
    }
}
