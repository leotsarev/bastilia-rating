namespace Bastilia.Rating.Database.Entities;

public class Achievement
{
    public int AchievementId { get; set; }
    public required TemplateId AchievementTemplateId { get; set; }
    public required UserIdentification UserId { get; set; }
    public required UserIdentification GrantedBy { get; set; }
    public DateOnly GrantedDate { get; set; }
    public UserIdentification? RemovedBy { get; set; }
    public DateOnly? RemovedDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }

    public string? OverrideName { get; set; }

    public required AchievementTemplate Template { get; set; }
    public required User User { get; set; }
    public required User GrantedByUser { get; set; }
    public required User? RemovedByUser { get; set; }
}
