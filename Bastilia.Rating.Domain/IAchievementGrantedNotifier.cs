namespace Bastilia.Rating.Domain;

public interface IAchievementGrantedNotifier
{
    Task NotifyAchievementGrantedAsync(BastiliaMember member, AchievementTemplate template, string projectName, string projectSlug, CancellationToken cancellationToken = default);
}
