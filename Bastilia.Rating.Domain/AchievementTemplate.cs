using System.Text.Json.Serialization;

namespace Bastilia.Rating.Domain
{
    [method: JsonConstructor]
    [TypedEntityId]
    public partial record class TemplateId(int Value)
    {
    }

    public record class AchievementTemplate(IBastiliaProjectLink? Project, string Name, string Description, bool DefaultUri, int RatingValue, bool YearlyAchievement, TemplateId TemplateId);
}