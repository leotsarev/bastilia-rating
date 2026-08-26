namespace Bastilia.Rating.Database.Entities;

public class ProjectAdmin
{
    public BastiliaProjectId ProjectId { get; set; } = null!;
    public UserIdentification UserId { get; set; } = null!;
    public DateOnly AddDate { get; set; }
    public DateOnly? RemoveDate { get; set; }

    public required BastiliaProject Project { get; set; }
    public required User User { get; set; }
}
