using Shotgun.Helpers;
using TEST.Shotgun.API.Fixtures;
using ShotgunRange = Shotgun.Expressions.Range;
using Xunit;

namespace TEST.Shotgun.API.Tests;

// NOTE: Range.cs currently has a bug — from and to are swapped when building
// the binary expressions (line 38-39). binaryExpression1 checks `>= to` and
// binaryExpression2 checks `<= from`, producing `date >= to AND date <= from`.
// The tests below assert the EXPECTED (correct) behavior: records whose date
// falls inside [from, to]. They will FAIL until the bug is fixed.
public abstract class RangeTestsBase
{
    protected readonly DatabaseFixture Fixture;
    protected RangeTestsBase(DatabaseFixture fixture) => Fixture = fixture;

    // --- null / empty dict ------------------------------------------------

    [Fact]
    public void RangeExpression_EmptyDict_ReturnsNull()
    {
        var expr = ShotgunRange.RangeExpression<object>(new Dictionary<string, string[]>());
        Assert.Null(expr);
    }

    [Fact]
    public void RangeExpression_EntryWithOneValue_SkipsEntry_ReturnsNull()
    {
        // Only entries with exactly 2 values are processed
        var dict = new Dictionary<string, string[]>
        {
            { "CreatedAt", new[] { "2024-01-01" } }
        };
        var expr = ShotgunRange.RangeExpression<object>(dict);
        Assert.Null(expr);
    }

    [Fact]
    public void RangeExpression_EntryWithThreeValues_SkipsEntry_ReturnsNull()
    {
        var dict = new Dictionary<string, string[]>
        {
            { "CreatedAt", new[] { "2024-01-01", "2024-06-01", "2024-12-31" } }
        };
        var expr = ShotgunRange.RangeExpression<object>(dict);
        Assert.Null(expr);
    }

    // --- non-nullable DateTime --------------------------------------------

    [Fact]
    public async Task DateRange_NonNullableDateTime_ReturnsEntitiesWithinRange()
    {
        // Seed: Alice=Jan15, Alice Smith=Feb5, Bob=Mar10, Dave=Apr1, Charlie=Jun20
        // Filter: Q1 2024 (Jan-Mar) → Alice, Alice Smith, Bob
        var page = new PagingQuery { PageNumber = 1, PageSize = 10 };
        var dateDict = new Dictionary<string, string[]>
        {
            { "CreatedAt", new[] { "2024-01-01", "2024-03-31" } }
        };
        var result = await Fixture.Repository.Search(page,
            new Dictionary<string, string[]>(),
            orderBy: null, asc: false, dateDict: dateDict);

        Assert.Equal(3, result.TotalCount);
        Assert.All(result, e =>
        {
            Assert.True(e.CreatedAt >= new DateTime(2024, 1, 1));
            Assert.True(e.CreatedAt <= new DateTime(2024, 3, 31));
        });
    }

    [Fact]
    public async Task DateRange_ExcludesBoundaryMisses()
    {
        // Only Charlie (Jun 20) falls after April
        var page = new PagingQuery { PageNumber = 1, PageSize = 10 };
        var dateDict = new Dictionary<string, string[]>
        {
            { "CreatedAt", new[] { "2024-05-01", "2024-12-31" } }
        };
        var result = await Fixture.Repository.Search(page,
            new Dictionary<string, string[]>(),
            orderBy: null, asc: false, dateDict: dateDict);

        Assert.Single(result);
        Assert.Equal("Charlie", result.First().Name);
    }

    // --- nullable DateTime ------------------------------------------------

    [Fact]
    public async Task DateRange_NullableDateTime_ReturnsEntitiesWithinRange()
    {
        // UpdatedAt: Alice=Feb1, Alice Smith=Feb10, Charlie=Jul1; Bob & Dave are null
        var page = new PagingQuery { PageNumber = 1, PageSize = 10 };
        var dateDict = new Dictionary<string, string[]>
        {
            { "UpdatedAt", new[] { "2024-01-01", "2024-03-01" } }
        };
        var result = await Fixture.Repository.Search(page,
            new Dictionary<string, string[]>(),
            orderBy: null, asc: false, dateDict: dateDict);

        // Only Alice (Feb1) and Alice Smith (Feb10) have UpdatedAt in range
        Assert.Equal(2, result.TotalCount);
    }

    // --- combined search + date range -------------------------------------

    [Fact]
    public async Task DateRange_CombinedWithSearchDict_AppliesBothFilters()
    {
        var page = new PagingQuery { PageNumber = 1, PageSize = 10 };
        var dict = new Dictionary<string, string[]> { { "IsActive", new[] { "true" } } };
        var dateDict = new Dictionary<string, string[]>
        {
            { "CreatedAt", new[] { "2024-01-01", "2024-03-31" } }
        };
        var result = await Fixture.Repository.Search(page, dict,
            orderBy: null, asc: false, dateDict: dateDict);

        // Active AND in Q1: Alice (Jan15), Alice Smith (Feb5)
        Assert.Equal(2, result.TotalCount);
        Assert.All(result, e => Assert.True(e.IsActive));
    }
}

[Collection("SqlServer")]
public class RangeTests_SqlServer : RangeTestsBase
{
    public RangeTests_SqlServer(SqlServerFixture fixture) : base(fixture) { }
}

[Collection("PostgreSql")]
public class RangeTests_PostgreSql : RangeTestsBase
{
    public RangeTests_PostgreSql(PostgreSqlFixture fixture) : base(fixture) { }
}

[Collection("MySql")]
public class RangeTests_MySql : RangeTestsBase
{
    public RangeTests_MySql(MySqlFixture fixture) : base(fixture) { }
}
