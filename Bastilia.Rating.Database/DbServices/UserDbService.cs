namespace Bastilia.Rating.Database.DbServices
{
    internal class UserDbService(IDbContextFactory<AppDbContext> contextFactory) : IUserDbService
    {
        public async Task<BastiliaMember> AddUser(UserIdentification playerId, string nickName, string avatarUrl)
        {
            var user = new Entities.User()
            {
                AvatarUrl = avatarUrl,
                Username = nickName,
                JoinRpgUserId = playerId,
                ParticipateInRating = true,
            };

            await using (var appDbContext = await contextFactory.CreateDbContextAsync())
            {
                await appDbContext.Set<Entities.User>().AddAsync(user);
                await appDbContext.SaveChangesAsync();
            }

            var rep = new BastiliaMemberRepository(contextFactory);
            return await rep.GetByIdAsync(playerId) ?? throw new InvalidOperationException();
        }
    }
}
