using System.Net;
using System.Net.Http.Json;
using System.Text;
using Bastilia.Rating.Domain.DomainServices;
using JoinRpg.Client;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.XGameApi.Contract;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Tests;

public class UserImportServiceTests
{
    private const long ExistingChatId = 123456789L;
    private const string ExistingUsername = "old_telegram";

    private static BastiliaMember CreateMember(string userName = "old", string avatarUrl = "https://example.com/old.png", TelegramSocialLink? telegram = null) =>
        new(1, userName, avatarUrl, "slug", true, [], [], [], null, telegram);

    private static TelegramSocialLink TelegramLink(long chatId = ExistingChatId, string? username = ExistingUsername)
        => TelegramSocialLink.FromOptional(chatId, username, isVerified: true)!;

    [Fact]
    public async Task ExistingUser_NickChanged_UpdatesNickOnly()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(userName: "new") };
        var importService = CreateImportService(CreateMember(userName: "old"), db);

        var result = await importService.ImportUser(1, "new", null);

        db.UpdateCalls.ShouldBe([(1, "new", (string?)null, (TelegramSocialLink?)null)]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
        result.UserName.ShouldBe("new");
    }

    [Fact]
    public async Task ExistingUser_AvatarChangedOnly_UpdatesAvatarOnly()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(avatarUrl: "https://a/64.png") };
        var importService = CreateImportService(CreateMember(), db);

        var result = await importService.ImportUser(1, null, "https://a/64.png");

        db.UpdateCalls.ShouldBe([(1, (string?)null, "https://a/64.png", (TelegramSocialLink?)null)]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
        result.AvatarUrl.ShouldBe("https://a/64.png");
    }

    [Fact]
    public async Task ExistingUser_DataMatches_NoUpdate()
    {
        var db = new FakeUserDbService();
        var member = CreateMember(userName: "same", avatarUrl: "https://a/64.png");
        var importService = CreateImportService(member, db);

        var result = await importService.ImportUser(1, "same", "https://a/64.png");

        db.UpdateCalls.ShouldBeEmpty();
        db.AddCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(member);
    }

    [Fact]
    public async Task NewUser_NickOnly_CreatesWithEmptyAvatar()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(userName: "nick", avatarUrl: "") };
        var importService = CreateImportService(existingMember: null, db);

        var result = await importService.ImportUser(1, "nick", null);

        db.AddCalls.ShouldBe([(1, "nick", "", (TelegramSocialLink?)null)]);
        db.UpdateCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task NewUser_NickAndAvatar_CreatesWithBoth()
    {
        var created = CreateMember(userName: "nick", avatarUrl: "https://a/64.png");
        var db = new FakeUserDbService { MemberToReturn = created };
        var importService = CreateImportService(existingMember: null, db);

        var result = await importService.ImportUser(1, "nick", "https://a/64.png");

        db.AddCalls.ShouldBe([(1, "nick", "https://a/64.png", (TelegramSocialLink?)null)]);
        db.UpdateCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(created);
    }

    [Fact]
    public async Task ExistingUser_BothChanged_UpdatesBoth()
    {
        var updated = CreateMember(userName: "new", avatarUrl: "https://a/64.png");
        var db = new FakeUserDbService { MemberToReturn = updated };
        var importService = CreateImportService(CreateMember(), db);

        var result = await importService.ImportUser(1, "new", "https://a/64.png");

        db.UpdateCalls.ShouldBe([(1, "new", "https://a/64.png", (TelegramSocialLink?)null)]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(updated);
    }

    [Fact]
    public async Task ExistingUser_TelegramChanged_UpdatesTelegramOnly()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(telegram: TelegramLink(username: "new_telegram")) };
        var importService = CreateImportService(CreateMember(telegram: TelegramLink()), db);

        var result = await importService.ImportUser(1, "old", "https://example.com/old.png", TelegramLink(ExistingChatId, "new_telegram"));

        db.UpdateCalls.ShouldBe([(1, "old", "https://example.com/old.png", TelegramLink(ExistingChatId, "new_telegram"))]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExistingUser_TelegramChatIdChanged_UpdatesChatIdOnly()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(telegram: TelegramLink(chatId: 987654321L)) };
        var importService = CreateImportService(CreateMember(telegram: TelegramLink()), db);

        var result = await importService.ImportUser(1, null, null, TelegramLink(987654321L, null));

        db.UpdateCalls.ShouldBe([(1, (string?)null, (string?)null, TelegramLink(987654321L, null))]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExistingUser_TelegramMatches_NoUpdate()
    {
        var db = new FakeUserDbService();
        var member = CreateMember(telegram: TelegramLink());
        var importService = CreateImportService(member, db);

        var result = await importService.ImportUser(1, null, null, TelegramLink());

        db.UpdateCalls.ShouldBeEmpty();
        db.AddCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(member);
    }

    [Fact]
    public async Task ExistingUser_NoTelegramClaims_KeepsExistingLink()
    {
        var db = new FakeUserDbService();
        var member = CreateMember(telegram: TelegramLink());
        var importService = CreateImportService(member, db);

        var result = await importService.ImportUser(1, "old", "https://example.com/old.png");

        db.UpdateCalls.ShouldBeEmpty();
        db.AddCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(member);
    }

    [Fact]
    public async Task ExistingUser_TelegramAndNickChanged_UpdatesBoth()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(userName: "new", telegram: TelegramLink(chatId: 987654321L)) };
        var importService = CreateImportService(CreateMember(telegram: TelegramLink()), db);

        var result = await importService.ImportUser(1, "new", null, TelegramLink(987654321L, null));

        db.UpdateCalls.ShouldBe([(1, "new", (string?)null, TelegramLink(987654321L, null))]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task NewUser_TelegramClaims_CreatesWithTelegram()
    {
        var created = CreateMember(userName: "nick", telegram: TelegramLink(chatId: 555L, username: "tg_user"));
        var db = new FakeUserDbService { MemberToReturn = created };
        var importService = CreateImportService(existingMember: null, db);

        var result = await importService.ImportUser(1, "nick", "https://a/64.png", TelegramLink(555L, "tg_user"));

        db.AddCalls.ShouldBe([(1, "nick", "https://a/64.png", TelegramLink(555L, "tg_user"))]);
        db.UpdateCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(created);
    }

    [Fact]
    public async Task ExistingUser_NullTelegramLink_NoUpdate()
    {
        var db = new FakeUserDbService();
        var member = CreateMember();
        var importService = CreateImportService(member, db);

        // Null-ссылка — например, claim telegram_chat_id отсутствовал (сборка ссылки — в BastiliaUserLoginHandler;
        // TelegramSocialLink без chatId не строится). Привязку не трогаем.
        var result = await importService.ImportUser(1, "old", "https://example.com/old.png", telegram: null);

        db.UpdateCalls.ShouldBeEmpty();
        db.AddCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(member);
    }

    [Fact]
    public async Task ExistingUser_TelegramChatIdMatches_UsernameClaimAbsent_KeepsDbUsername()
    {
        var db = new FakeUserDbService();
        var member = CreateMember(telegram: TelegramLink());
        var importService = CreateImportService(member, db);

        // Отсутствие claim telegram не затирает имя в БД: отсутствие claim ≠ отвязка.
        var result = await importService.ImportUser(1, "old", "https://example.com/old.png", TelegramLink(ExistingChatId, null));

        db.UpdateCalls.ShouldBeEmpty();
        db.AddCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(member);
    }

    [Fact]
    public async Task ImportUserById_ExistingUser_TelegramChatIdChanged_UpdatesTelegram()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(telegram: TelegramLink(chatId: 987654321L, username: "new_telegram")) };
        var importService = CreateImportService(CreateMember(telegram: TelegramLink()), db);

        var result = await importService.ImportUserById(1, TelegramLink(987654321L, "new_telegram"));

        db.UpdateCalls.ShouldBe([(1, (string?)null, (string?)null, TelegramLink(987654321L, "new_telegram"))]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task ImportUserById_NullTelegramLink_NoUpdate()
    {
        var db = new FakeUserDbService();
        var member = CreateMember();
        var importService = CreateImportService(member, db);

        var result = await importService.ImportUserById(1, telegram: null);

        db.UpdateCalls.ShouldBeEmpty();
        db.AddCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(member);
    }

    [Fact]
    public async Task ImportUserById_NewUser_TelegramClaims_CreatesWithTelegram()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(userName: "nick", avatarUrl: "https://a/64.png", telegram: TelegramLink(chatId: 555L, username: "tg_user")) };
        var importService = CreateImportService(existingMember: null, db, CreateJoinUserInfoClient());

        var result = await importService.ImportUserById(1, TelegramLink(555L, "tg_user"));

        db.AddCalls.ShouldBe([(1, "nick", "https://a/64.png", TelegramLink(555L, "tg_user"))]);
        db.UpdateCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
    }

    private static JoinUserInfoClient CreateJoinUserInfoClient()
    {
        var httpClient = new HttpClient(new StubJoinHttpHandler());
        var options = Options.Create(new JoinConnectOptions { Host = "https://joinrpg.ru", UserName = "user", Password = "password" });
        return new JoinUserInfoClient(httpClient, options, NullLogger<JoinUserInfoClient>.Instance);
    }

    private sealed class StubJoinHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/x-api/token", StringComparison.Ordinal))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"access_token":"token","token_type":"Bearer","expires_in":3600}""", Encoding.UTF8, "application/json"),
                });
            }

            var playerInfo = new PlayerInfo(1, "nick", "Full Name", "https://a/64.png", new PlayerContacts("a@b.c", null, null, null));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(playerInfo),
            });
        }
    }

    private static UserImportService CreateImportService(BastiliaMember? existingMember, FakeUserDbService dbService, JoinUserInfoClient? joinUserInfoClient = null)
        => new(new FakeMemberRepository(existingMember), joinUserInfoClient ?? null!, dbService, NullLogger<UserImportService>.Instance);

    private sealed class FakeUserDbService : IUserDbService
    {
        public List<(int Id, string? Nick, string? Avatar, TelegramSocialLink? Telegram)> UpdateCalls { get; } = [];
        public List<(int Id, string Nick, string Avatar, TelegramSocialLink? Telegram)> AddCalls { get; } = [];

        public BastiliaMember? MemberToReturn { get; set; }

        public Task<BastiliaMember> AddUser(int playerId, string nickName, string avatarUrl, TelegramSocialLink? telegram = null)
        {
            AddCalls.Add((playerId, nickName, avatarUrl, telegram));
            return Task.FromResult(MemberToReturn ?? throw new InvalidOperationException("MemberToReturn не задан"));
        }

        public Task<BastiliaMember> UpdateUser(int playerId, string? nickName, string? avatarUrl, TelegramSocialLink? telegram = null)
        {
            UpdateCalls.Add((playerId, nickName, avatarUrl, telegram));
            return Task.FromResult(MemberToReturn ?? throw new InvalidOperationException("MemberToReturn не задан"));
        }
    }
}
