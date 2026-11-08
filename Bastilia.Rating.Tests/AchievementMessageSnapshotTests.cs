using Bastilia.Rating.Domain;
using Bastilia.Rating.Portal.Common.Telegram;
using VerifyXunit;
using AchievementTemplate = Bastilia.Rating.Domain.AchievementTemplate;

namespace Bastilia.Rating.Tests;

public class AchievementMessageSnapshotTests : VerifyBase
{
    public AchievementMessageSnapshotTests() : base()
    {
    }

    [Fact]
    public Task BuildMessage_ProducesStableNotification()
    {
        var template = new AchievementTemplate(
            Project: null, Name: "Спасатель <Моржов>",
            Description: "desc", DefaultUri: false,
            RatingValue: 0, YearlyAchievement: false, TemplateId: 7);

        var message = new AchievementMessageBuilder()
            .BuildMessage("Имя & Фамилия", template, "Проект «Рассвет»", "rassvet", "rating.bastilia.ru");

        return Verify(message);
    }
}
