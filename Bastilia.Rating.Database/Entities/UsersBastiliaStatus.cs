namespace Bastilia.Rating.Database.Entities;

public class UsersBastiliaStatus
{
    public required UserIdentification JoinrpgUserId { get; set; }
    public DateOnly BeginDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public BastiliaStatusType StatusType { get; set; }

    [ForeignKey(nameof(JoinrpgUserId))]
    public required User User { get; set; }
}
