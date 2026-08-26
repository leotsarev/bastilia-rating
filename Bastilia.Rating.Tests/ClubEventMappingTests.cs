namespace JoinRpg.PrimitiveTypes.Test;

public class ClubEventMappingTests
{
    private static readonly DateOnly TestDate = new(2026, 3, 7);

    [Fact]
    public void BirthdayParty_MapsCorrectly()
    {
        var clubEvent = new ClubEvent
        {
            ClubEventId = 1,
            EventType = BastiliaCalendarItemType.BirthdayParty,
            EventDate = TestDate,
            Length = 2,
            JoinRpgUserId = new UserIdentification(42),
            User = new User { JoinRpgUserId = new UserIdentification(42), Username = "Иванов", AvatarUrl = "" },
        };

        var item = BastiliaMemberRepository.ToCalendarItem(clubEvent);

        item.Type.ShouldBe(BastiliaCalendarItemType.BirthdayParty);
        item.Name.ShouldBe("Иванов");
        item.Id.ShouldBe(42);
        item.StartDate.ShouldBe(TestDate);
        item.EndDate.ShouldBe(TestDate.AddDays(1));
    }

    [Fact]
    public void BastilleDinner_MapsCorrectly()
    {
        var clubEvent = new ClubEvent
        {
            ClubEventId = 10,
            EventType = BastiliaCalendarItemType.BastiliaDinner,
            EventDate = TestDate,
            Length = 1,
            Title = "Осенний ужин",
        };

        var item = BastiliaMemberRepository.ToCalendarItem(clubEvent);

        item.Type.ShouldBe(BastiliaCalendarItemType.BastiliaDinner);
        item.Name.ShouldBe("Осенний ужин");
        item.Id.ShouldBe(10);
        item.StartDate.ShouldBe(TestDate);
        item.EndDate.ShouldBe(TestDate);
    }

    [Fact]
    public void PartyEvent_MapsCorrectly()
    {
        var clubEvent = new ClubEvent
        {
            ClubEventId = 20,
            EventType = BastiliaCalendarItemType.PartyEvent,
            EventDate = TestDate,
            Length = 3,
            Title = "Летняя вечеринка",
        };

        var item = BastiliaMemberRepository.ToCalendarItem(clubEvent);

        item.Type.ShouldBe(BastiliaCalendarItemType.PartyEvent);
        item.Name.ShouldBe("Летняя вечеринка");
        item.Id.ShouldBe(20);
        item.StartDate.ShouldBe(TestDate);
        item.EndDate.ShouldBe(TestDate.AddDays(2));
    }

    [Fact]
    public void ProjectGathering_MapsCorrectly()
    {
        var clubEvent = new ClubEvent
        {
            ClubEventId = 30,
            EventType = BastiliaCalendarItemType.ProjectGathering,
            EventDate = TestDate,
            Length = 1,
            Title = "Сбор команды",
            ProjectId = new BastiliaProjectId(55),
        };

        var item = BastiliaMemberRepository.ToCalendarItem(clubEvent);

        item.Type.ShouldBe(BastiliaCalendarItemType.ProjectGathering);
        item.Name.ShouldBe("Сбор команды");
        item.Id.ShouldBe(55);
        item.StartDate.ShouldBe(TestDate);
        item.EndDate.ShouldBe(TestDate);
    }
}
