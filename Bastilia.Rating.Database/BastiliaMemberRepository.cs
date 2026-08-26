using LinqKit;

namespace Bastilia.Rating.Database;

internal class BastiliaMemberRepository(IDbContextFactory<AppDbContext> contextFactory) : BastiliaRepositoryBase, IBastiliaMemberRepository
{
    public async Task<BastiliaMember?> GetByIdAsync(UserIdentification userId) => (await GetMemberImpl(u => u.JoinRpgUserId == userId)).FirstOrDefault();

    public async Task<BastiliaMember?> GetBySlugAsync(string slug) => (await GetMemberImpl(u => u.Slug == slug)).FirstOrDefault();

    public Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync() => GetMemberImpl(u => true);

    public async Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var actualStatusPredicate = GetActualStatusPredicate();
        var users = await MemberQuery(context)
                    .AsNoTracking()
                    .Where(u => actualStatusPredicate.Invoke(u))
                    .ToArrayAsync();

        return [.. users.Select(ToMember).Where(x => x.CurrentStatus == BastiliaFinalStatus.Active)];
    }

    public async Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var users = await context.UsersBastiliaStatuses
                    .Include(s => s.User)
                    .Where(s => s.StatusType == BastiliaStatusType.Member)
                    .ToArrayAsync();
        return [.. users.Select(s => new MemberHistoryItem(ToUserLink(s.User), s.BeginDate, s.EndDate))];
    }

    public async Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var actualStatusPredicate = GetActualStatusPredicate();
        var birthdays = await MemberQuery(context)
            .AsNoTracking()
            .Where(u => actualStatusPredicate.Invoke(u))
            .Where(u => u.BirthDay != null)
            .Select(u => new { u.Username, BirthDay = u.BirthDay!.Value, u.JoinRpgUserId })
            .ToArrayAsync();

        var clubEvents = await context.ClubEvents
            .Where(ce => ce.EventDate.Year == year)
            .Include(ce => ce.User)
            .Include(ce => ce.Project)
            .ToArrayAsync();

        return [
            ..birthdays.Select(b => new BastiliaCalendarItem(BastiliaCalendarItemType.Birthday, new DateOnly(year, b.BirthDay.Month, b.BirthDay.Day), b.Username, b.JoinRpgUserId)),
            ..clubEvents.Select(ToCalendarItem)
            ];
    }

    private async Task<IReadOnlyCollection<BastiliaMember>> GetMemberImpl(Expression<Func<Entities.User, bool>> predicate)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var actualStatusPredicate = GetActualStatusPredicate();
        var users = await MemberQuery(context)
                    .AsNoTracking()
                    .Select(u => new { User = u, CurrentStatus = actualStatusPredicate.Invoke(u) })
                    .Where(u => predicate.Invoke(u.User))
                    .ToArrayAsync();

        return [.. users.Select(u => ToMember(u.User))];
    }

    private static Expression<Func<Entities.User, bool>> GetActualStatusPredicate()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return u => u.BastiliaStatuses.Where(s => s.EndDate == null || s.EndDate > today).Where(s => s.BeginDate < today).OrderBy(s => s.BeginDate).Any();
    }

    private static IQueryable<Entities.User> MemberQuery(AppDbContext context)
    {
        return context.Users
                            .AsExpandable()
                            .Include(u => u.BastiliaStatuses)
                            .Include(u => u.ProjectAdmins)
                                .ThenInclude(pa => pa.Project)
                            .Include(u => u.Achievements)
                                .ThenInclude(a => a.Template)
                                .ThenInclude(a => a.Project)
                            .Include(u => u.Achievements)
                                .ThenInclude(a => a.GrantedByUser)
                            .Include(u => u.Achievements)
                                .ThenInclude(a => a.RemovedByUser);
    }

    internal static BastiliaCalendarItem ToCalendarItem(Entities.ClubEvent e)
    {
        var endDate = e.EventDate.AddDays(e.Length - 1);
        return e.EventType switch
        {
            BastiliaCalendarItemType.BirthdayParty => new BastiliaCalendarItem(
                BastiliaCalendarItemType.BirthdayParty, e.EventDate, endDate, e.User!.Username, e.JoinRpgUserId!.Value),
            BastiliaCalendarItemType.ProjectGathering => new BastiliaCalendarItem(
                BastiliaCalendarItemType.ProjectGathering, e.EventDate, endDate, e.Title ?? "", e.ProjectId!.Value),
            BastiliaCalendarItemType.BastiliaDinner => new BastiliaCalendarItem(
                BastiliaCalendarItemType.BastiliaDinner, e.EventDate, endDate, e.Title ?? "", e.ClubEventId),
            BastiliaCalendarItemType.PartyEvent => new BastiliaCalendarItem(
                BastiliaCalendarItemType.PartyEvent, e.EventDate, endDate, e.Title ?? "", e.ClubEventId),
            _ => new BastiliaCalendarItem(
                BastiliaCalendarItemType.Unknown, e.EventDate, endDate, e.Title ?? "", e.ClubEventId),
        };
    }

    private static BastiliaMember ToMember(Entities.User user)
    {
        IUserLink userAsLink = ToUserLink(user);
        return new BastiliaMember(
            JoinrpgUserId: user.JoinRpgUserId,
            UserName: user.Username,
            AvatarUrl: user.AvatarUrl,
            Slug: user.Slug,
            ParticipateInRating: user.ParticipateInRating,
            StatusHistory: user.BastiliaStatuses
                .Select(s => new BastiliaStatusHistory(
                    s.BeginDate,
                    s.StatusType,
                    s.EndDate))
                .ToList()
                .AsReadOnly(),
            HisProjects: user.ProjectAdmins
                .Select(pa => new ProjectAdminInfo(
                    pa.ProjectId,
                    pa.Project.ProjectName,
                    pa.AddDate,
                    pa.RemoveDate))
                .ToList()
                .AsReadOnly(),
            Achievements: user.Achievements
                .Where(a => a.RemovedDate == null)
                .Select(a => ToMemberAchievement(a))
                .ToList()
                .AsReadOnly(),
            BirthDay: user.BirthDay);
    }
}
