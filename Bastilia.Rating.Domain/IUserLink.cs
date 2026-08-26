namespace Bastilia.Rating.Domain
{
    public interface IUserLink
    {
        UserIdentification JoinrpgUserId { get; }
        string? Slug { get; }
        string UserName { get; }
    }
}