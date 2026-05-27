using Microsoft.EntityFrameworkCore;
using Shotgun.Expressions;
using TEST.Shotgun.API.Domain;
using TEST.Shotgun.API.Fixtures;
using Xunit;

namespace TEST.Shotgun.API.Tests;

// ---------------------------------------------------------------------------
// Abstract base — all assertions live here, once
// ---------------------------------------------------------------------------
public abstract class SearchTestsBase
{
    protected readonly DatabaseFixture Fixture;
    protected SearchTestsBase(DatabaseFixture fixture) => Fixture = fixture;

    // --- string Contains ---------------------------------------------------

    [Fact]
    public async Task SearchByName_PartialString_ReturnsAllMatches()
    {
        var dict = new Dictionary<string, string[]> { { "Name", new[] { "alice" } } };
        // Case-sensitive Contains — "alice" matches "Alice" and "Alice Smith"
        // because SQL LIKE is typically case-insensitive on most providers
        var result = await Fixture.Repository.Search(dict);
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Contains("Alice", e.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchByName_ArrayValues_AppliesOrLogic()
    {
        var dict = new Dictionary<string, string[]> { { "Name", new[] { "Alice", "Bob" } } };
        var result = await Fixture.Repository.Search(dict);
        // OR: "Alice" | "Bob" | "Alice Smith"
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchMultipleFields_AppliesAndLogic()
    {
        // Name contains "Alice" AND IsActive = true → Alice, Alice Smith (both active)
        var dict = new Dictionary<string, string[]>
        {
            { "Name",     new[] { "Alice" } },
            { "IsActive", new[] { "true"  } },
        };
        var result = await Fixture.Repository.Search(dict);
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.True(e.IsActive));
        Assert.All(result, e => Assert.Contains("Alice", e.Name, StringComparison.OrdinalIgnoreCase));
    }

    // --- int ---------------------------------------------------------------

    [Fact]
    public async Task SearchByAge_IntEquals_ReturnsMatch()
    {
        var dict = new Dictionary<string, string[]> { { "Age", new[] { "30" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public async Task SearchByAge_IntMultipleValues_ReturnsMatches()
    {
        var dict = new Dictionary<string, string[]> { { "Age", new[] { "25", "35" } } };
        var result = await Fixture.Repository.Search(dict);
        // Age 25: Bob, Dave — Age 35: Charlie
        Assert.Equal(3, result.Count);
    }

    // --- int? --------------------------------------------------------------

    [Fact]
    public async Task SearchByNullableAge_NullableInt_ReturnsMatch()
    {
        var dict = new Dictionary<string, string[]> { { "NullableAge", new[] { "25" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    // --- long --------------------------------------------------------------

    [Fact]
    public async Task SearchByScore_LongEquals_ReturnsMatch()
    {
        var dict = new Dictionary<string, string[]> { { "Score", new[] { "2000" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Single(result);
        Assert.Equal("Bob", result[0].Name);
    }

    // --- long? -------------------------------------------------------------

    [Fact]
    public async Task SearchByNullableScore_NullableLong_ReturnsMatch()
    {
        var dict = new Dictionary<string, string[]> { { "NullableScore", new[] { "500" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    // --- short -------------------------------------------------------------

    [Fact]
    public async Task SearchByRating_ShortEquals_ReturnsMatches()
    {
        var dict = new Dictionary<string, string[]> { { "Rating", new[] { "5" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Equal(2, result.Count); // Alice, Alice Smith
    }

    // --- short? ------------------------------------------------------------

    [Fact]
    public async Task SearchByNullableRating_NullableShort_ReturnsMatch()
    {
        var dict = new Dictionary<string, string[]> { { "NullableRating", new[] { "4" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    // --- bool --------------------------------------------------------------

    [Fact]
    public async Task SearchByIsActive_True_ReturnsActiveEntities()
    {
        var dict = new Dictionary<string, string[]> { { "IsActive", new[] { "true" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Equal(3, result.Count); // Alice, Charlie, Alice Smith
        Assert.All(result, e => Assert.True(e.IsActive));
    }

    [Fact]
    public async Task SearchByIsActive_False_ReturnsInactiveEntities()
    {
        var dict = new Dictionary<string, string[]> { { "IsActive", new[] { "false" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Equal(2, result.Count); // Bob, Dave
        Assert.All(result, e => Assert.False(e.IsActive));
    }

    // --- bool? -------------------------------------------------------------

    [Fact]
    public async Task SearchByIsVerified_NullableBoolTrue_ReturnsMatches()
    {
        var dict = new Dictionary<string, string[]> { { "IsVerified", new[] { "true" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Equal(2, result.Count); // Alice, Alice Smith
    }

    // --- Guid --------------------------------------------------------------

    [Fact]
    public async Task SearchByExternalId_GuidEquals_ReturnsSingleEntity()
    {
        var dict = new Dictionary<string, string[]>
        {
            { "ExternalId", new[] { SeedData.Guid1.ToString() } }
        };
        var result = await Fixture.Repository.Search(dict);
        Assert.Single(result);
        Assert.Equal(SeedData.Guid1, result[0].ExternalId);
    }

    // --- Guid? -------------------------------------------------------------

    [Fact]
    public async Task SearchByNullableExternalId_NullableGuid_ReturnsMatch()
    {
        var dict = new Dictionary<string, string[]>
        {
            { "NullableExternalId", new[] { SeedData.NullableGuid1.ToString() } }
        };
        var result = await Fixture.Repository.Search(dict);
        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    // --- empty / unknown field ---------------------------------------------

    [Fact]
    public async Task Search_EmptyDict_ReturnsAllEntities()
    {
        // ContainsValues returns null for empty dict → GetQueryWithInclude uses no filter
        var result = await Fixture.Repository.Search(new Dictionary<string, string[]>());
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task Search_UnknownField_ReturnsAllEntities()
    {
        var dict = new Dictionary<string, string[]> { { "NoSuchField", new[] { "x" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Equal(5, result.Count);
    }

    // --- key casing --------------------------------------------------------

    [Fact]
    public async Task Search_LowerCaseKey_MatchesCaseInsensitively()
    {
        var dict = new Dictionary<string, string[]> { { "isactive", new[] { "true" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Equal(3, result.Count);
    }

    // --- FirstCharToUpper edge cases (pure expression, no DB) --------------

    [Fact]
    public void FirstCharToUpper_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Search.FirstCharToUpper(null!));
    }

    [Fact]
    public void FirstCharToUpper_EmptyInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => Search.FirstCharToUpper(string.Empty));
    }

    [Fact]
    public void FirstCharToUpper_LowerCase_CapitalizesFirstChar()
    {
        Assert.Equal("Name", Search.FirstCharToUpper("name"));
    }
}

// ---------------------------------------------------------------------------
// Per-provider concrete classes
// ---------------------------------------------------------------------------

[Collection("SqlServer")]
public class SearchTests_SqlServer : SearchTestsBase
{
    public SearchTests_SqlServer(SqlServerFixture fixture) : base(fixture) { }
}

[Collection("PostgreSql")]
public class SearchTests_PostgreSql : SearchTestsBase
{
    public SearchTests_PostgreSql(PostgreSqlFixture fixture) : base(fixture) { }
}

[Collection("MySql")]
public class SearchTests_MySql : SearchTestsBase
{
    public SearchTests_MySql(MySqlFixture fixture) : base(fixture) { }
}
