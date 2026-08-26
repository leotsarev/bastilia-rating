namespace Bastilia.Rating.Database.Entities;

public class ClubEvent
{
    public int ClubEventId { get; set; }
    public BastiliaCalendarItemType EventType { get; set; } = BastiliaCalendarItemType.BirthdayParty;
    public DateOnly EventDate { get; set; }
    public int Length { get; set; } = 1;

    // Для BirthdayParty
    public UserIdentification? JoinRpgUserId { get; set; }
    public User? User { get; set; }

    // Для ProjectGathering
    public BastiliaProjectId? ProjectId { get; set; }
    public BastiliaProject? Project { get; set; }

    // Для BastilleDinner, PartyEvent и ProjectGathering
    public string? Title { get; set; }
}
