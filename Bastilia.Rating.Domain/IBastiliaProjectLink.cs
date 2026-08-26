namespace Bastilia.Rating.Domain
{
    public interface IBastiliaProjectLink
    {
        BastiliaProjectId BastiliaProjectId { get; }
        string ProjectName { get; }
        string? Slug { get; }
    }
}