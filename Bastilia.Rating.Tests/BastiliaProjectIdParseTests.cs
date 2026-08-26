namespace JoinRpg.PrimitiveTypes.Test;

public class BastiliaProjectIdParseTests
{
    [Fact]
    public void Slug_DoesNotParse()
    {
        BastiliaProjectId.TryParse("some-slug", null, out _).ShouldBeFalse();
    }

    [Fact]
    public void NumericString_Parses()
    {
        BastiliaProjectId.TryParse("123", null, out var result).ShouldBeTrue();
        result.Value.ShouldBe(123);
    }
}
