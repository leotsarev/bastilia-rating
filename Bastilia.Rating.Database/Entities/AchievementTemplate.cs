namespace Bastilia.Rating.Database.Entities;

public class AchievementTemplate
{
    public TemplateId AchievementTemplateId { get; set; } = null!;
    public BastiliaProjectId? ProjectId { get; set; }
    public UserIdentification? OwnerId { get; set; }
    public required string AchievementName { get; set; }
    public required string AchievementDescription { get; set; }
    public required string AchievementImageUrl { get; set; }
    public int AchievementRatingValue { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    public required BastiliaProject Project { get; set; }
    public required User Owner { get; set; }

    public required bool YearlyAchievement { get; set; } = false;
    public ICollection<Achievement> Achievements { get; set; } = [];
}
