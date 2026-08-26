using JoinRpg.Client;
using Microsoft.Extensions.Logging;

namespace Bastilia.Rating.Domain.DomainServices;

public class UserImportService(IBastiliaMemberRepository bastiliaMemberRepository, JoinUserInfoClient joinUserInfoClient, IUserDbService userDbService,
    ILogger<UserImportService> logger)
{
    public async Task<BastiliaMember?> ImportUser(UserIdentification userId)
    {
        var user = await bastiliaMemberRepository.GetByIdAsync(userId);
        if (user is not null)
        {
            return user;
        }

        var info = await joinUserInfoClient.GetUserInfo(userId.Value);
        if (info is not null)
        {
            return await userDbService.AddUser(new UserIdentification(info.PlayerId), info.NickName, info.AvatarUrl);
        }

        logger.LogWarning("Не удалось загрузить пользователя {userId}", userId);
        return null;
    }
}
