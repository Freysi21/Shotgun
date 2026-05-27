using Microsoft.EntityFrameworkCore;
using Shotgun.Expressions;
using Shotgun.Helpers;
using TEST.Shotgun.API.Domain;
using TEST.Shotgun.API.Fixtures;
using Xunit;

namespace TEST.Shotgun.API.Tests;

public abstract class ThenByTestsBase
{
    protected readonly DatabaseFixture Fixture;
    protected ThenByTestsBase(DatabaseFixture fixture) => Fixture = fixture;

    // --- ThenByProperty ---------------------------------------------------

    [Fact]
    public async Task ThenByProperty_SecondaryAscending_BreaksTies()
    {
        // Primary: Age asc (25 appears twice — Bob, Dave); secondary: Name asc
        // OrderByProperty returns IQueryable<T>, so cast to IOrderedQueryable<T> before ThenBy
        var base_query = Fixture.Context.Entities.AsQueryable()
            .OrderByProperty<TestEntity>("Age")!;
        var ordered = ((IOrderedQueryable<TestEntity>)base_query)
            .ThenByProperty<TestEntity>("Name")!;

        var result = await ordered.ToListAsync();

        var age25 = result.Where(e => e.Age == 25).Select(e => e.Name).ToList();
        Assert.Equal(age25.OrderBy(n => n, StringComparer.Ordinal), age25);
    }

    [Fact]
    public async Task ThenByPropertyDescending_SecondaryDescending_BreaksTies()
    {
        var base_query = Fixture.Context.Entities.AsQueryable()
            .OrderByProperty<TestEntity>("Age")!;
        var ordered = ((IOrderedQueryable<TestEntity>)base_query)
            .ThenByPropertyDescending<TestEntity>("Name")!;

        var result = await ordered.ToListAsync();

        var age25 = result.Where(e => e.Age == 25).Select(e => e.Name).ToList();
        Assert.Equal(age25.OrderByDescending(n => n, StringComparer.Ordinal), age25);
    }

    [Fact]
    public async Task ThenByProperty_MultipleLevels_AllApplied()
    {
        // Primary: IsActive desc, secondary: Rating desc, tertiary: Age asc
        var base_query = Fixture.Context.Entities.AsQueryable()
            .OrderByPropertyDescending<TestEntity>("IsActive")!;
        var ordered = ((IOrderedQueryable<TestEntity>)base_query)
            .ThenByPropertyDescending<TestEntity>("Rating")!
            .ThenByProperty<TestEntity>("Age")!;

        var result = await ordered.ToListAsync();
        Assert.Equal(5, result.Count);

        // Active entities come first
        var firstInactive = result.FindIndex(e => !e.IsActive);
        Assert.True(result.Take(firstInactive).All(e => e.IsActive));
    }

    // --- null return on missing property ----------------------------------

    [Fact]
    public void ThenByProperty_NonExistentProperty_ReturnsNull()
    {
        var base_query = Fixture.Context.Entities.AsQueryable()
            .OrderByProperty<TestEntity>("Age")!;
        var ordered = (IOrderedQueryable<TestEntity>)base_query;

        Assert.Null(ordered.ThenByProperty<TestEntity>("NoSuchProperty"));
    }

    [Fact]
    public void ThenByPropertyDescending_NonExistentProperty_ReturnsNull()
    {
        var base_query = Fixture.Context.Entities.AsQueryable()
            .OrderByProperty<TestEntity>("Age")!;
        var ordered = (IOrderedQueryable<TestEntity>)base_query;

        Assert.Null(ordered.ThenByPropertyDescending<TestEntity>("NoSuchProperty"));
    }

    // --- via Repository with orderByDict ----------------------------------

    [Fact]
    public async Task Repository_Search_WithOrderByDict_MultiColumnSort()
    {
        var page = new PagingQuery { PageNumber = 1, PageSize = 10 };
        var orderByDict = new Dictionary<string, bool>
        {
            { "Age",  true  },  // primary: Age asc
            { "Name", true  },  // secondary: Name asc
        };
        var result = await Fixture.Repository.Search(page,
            new Dictionary<string, string[]>(),
            new Dictionary<string, string[]>(),
            orderByDict);

        Assert.Equal(5, result.TotalCount);
        var ages = result.Select(e => e.Age).ToList();
        for (int i = 1; i < ages.Count; i++)
            Assert.True(ages[i] >= ages[i - 1]);
    }
}

[Collection("SqlServer")]
public class ThenByTests_SqlServer : ThenByTestsBase
{
    public ThenByTests_SqlServer(SqlServerFixture fixture) : base(fixture) { }
}

[Collection("PostgreSql")]
public class ThenByTests_PostgreSql : ThenByTestsBase
{
    public ThenByTests_PostgreSql(PostgreSqlFixture fixture) : base(fixture) { }
}

[Collection("MySql")]
public class ThenByTests_MySql : ThenByTestsBase
{
    public ThenByTests_MySql(MySqlFixture fixture) : base(fixture) { }
}
