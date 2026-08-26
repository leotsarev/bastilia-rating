namespace JoinRpg.PrimitiveTypes.Test;

public class BastiliaMemberRolesTests
{
    private static BastiliaMember CreateMember(params BastiliaStatusHistory[] statuses) =>
        new(new UserIdentification(1), "test", "https://example.com/avatar.png", "test", true,
            statuses, [], [], null);

    [Fact]
    public void NoStatuses_NotActiveMember_NotPresident()
    {
        var member = CreateMember();

        member.IsActiveMember.ShouldBeFalse();
        member.IsPresident.ShouldBeFalse();
    }

    [Fact]
    public void ActiveMemberStatus_IsActiveMember()
    {
        var member = CreateMember(
            new BastiliaStatusHistory(
                DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                BastiliaStatusType.Member,
                EndDate: null));

        member.IsActiveMember.ShouldBeTrue();
        member.IsPresident.ShouldBeFalse();
    }

    [Fact]
    public void ExpiredMemberStatus_IsRetired()
    {
        var member = CreateMember(
            new BastiliaStatusHistory(
                DateOnly.FromDateTime(DateTime.Now.AddDays(-365)),
                BastiliaStatusType.Member,
                DateOnly.FromDateTime(DateTime.Now.AddDays(-30))));

        member.IsActiveMember.ShouldBeFalse();
        member.CurrentStatus.ShouldBe(BastiliaFinalStatus.Retired);
    }

    [Fact]
    public void ActivePresidentStatus_IsPresident()
    {
        var member = CreateMember(
            new BastiliaStatusHistory(
                DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                BastiliaStatusType.President,
                EndDate: null));

        member.IsPresident.ShouldBeTrue();
    }

    [Fact]
    public void ExpiredPresidentStatus_NotPresident()
    {
        var member = CreateMember(
            new BastiliaStatusHistory(
                DateOnly.FromDateTime(DateTime.Now.AddDays(-365)),
                BastiliaStatusType.President,
                DateOnly.FromDateTime(DateTime.Now.AddDays(-30))));

        member.IsPresident.ShouldBeFalse();
    }

    [Fact]
    public void ActiveMemberAndPresident_BothTrue()
    {
        var member = CreateMember(
            new BastiliaStatusHistory(
                DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                BastiliaStatusType.Member,
                EndDate: null),
            new BastiliaStatusHistory(
                DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                BastiliaStatusType.President,
                EndDate: null));

        member.IsActiveMember.ShouldBeTrue();
        member.IsPresident.ShouldBeTrue();
    }
}
