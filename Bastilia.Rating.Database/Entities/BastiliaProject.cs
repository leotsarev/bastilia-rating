namespace Bastilia.Rating.Database.Entities;

public class BastiliaProject
{
    public BastiliaProjectId BastiliaProjectId { get; set; } = null!;
    public required string ProjectName { get; set; }

    public string ProjectDescription { get; set; } = "";
    public string HowToHelp { get; set; } = "";
    public ProjectType ProjectType { get; set; }
    public BrandType BrandType { get; set; }
    public bool OngoingProject { get; set; }
    public DateOnly CreateDate { get; set; }
    public DateOnly PlannedStartDate { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? PlannedEndDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool? ProjectOfTheYear { get; set; }

    public int? JoinrpgProjectId { get; set; }
    public int? KogdaIgraProjectId { get; set; }
    public string? ProjectUri { get; set; }

    public string? Slug { get; set; }

    public ICollection<ProjectAdmin> ProjectAdmins { get; set; } = [];
    public ICollection<AchievementTemplate> AchievementTemplates { get; set; } = [];
    public required string ProjectIconUri { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}
