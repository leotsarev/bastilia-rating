namespace Bastilia.Rating.Database.Entities;

[PrimaryKey(nameof(JoinRpgUserId))]
public class User
{

    public required UserIdentification JoinRpgUserId { get; set; }
    public required string Username { get; set; }
    public required string AvatarUrl { get; set; }
    public bool ParticipateInRating { get; set; }

    public string? Slug { get; set; }

    public DateOnly? BirthDay { get; set; }

    public ICollection<ProjectAdmin> ProjectAdmins { get; set; } = [];
    public ICollection<AchievementTemplate> OwnedAchievementTemplates { get; set; } = [];
    public ICollection<Achievement> Achievements { get; set; } = [];
    public ICollection<UsersBastiliaStatus> BastiliaStatuses { get; set; } = [];

    public ICollection<ClubEvent> ClubEvents { get; set; } = [];
}
