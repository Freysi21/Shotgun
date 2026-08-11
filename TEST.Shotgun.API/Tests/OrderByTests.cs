using Microsoft.EntityFrameworkCore;
using Shotgun.Expressions;
using Shotgun.Helpers;
using TEST.Shotgun.API.Domain;
using TEST.Shotgun.API.Fixtures;
using Xunit;

namespace TEST.Shotgun.API.Tests;

public abstract class OrderByTestsBase
{
    protected readonly DatabaseFixture Fixture;
    protected OrderByTestsBase(DatabaseFixture fixture) => Fixture = fixture;

    // --- PropertyExists ----------------------------------------------------

    [Fact]
    public void PropertyExists_KnownProperty_ReturnsTrue()
    {
        var query = Fixture.Context.Entities.AsQueryable();
        Assert.True(query.PropertyExists("Name"));
    }

    [Fact]
    public void PropertyExists_UnknownProperty_ReturnsFalse()
    {
        var query = Fixture.Context.Entities.AsQueryable();
        Assert.False(query.PropertyExists("NoSuchProperty"));
    }

    [Fact]
    public void PropertyExists_CaseInsensitive_ReturnsTrue()
    {
        var query = Fixture.Context.Entities.AsQueryable();
        Assert.True(query.PropertyExists("name"));
        Assert.True(query.PropertyExists("NAME"));
    }

    // --- OrderByProperty (ascending) --------------------------------------

    [Fact]
    public async Task OrderByProperty_Ascending_SortsAscending()
    {
        var query = Fixture.Context.Entities.AsQueryable();
        var ordered = query.OrderByProperty("Age")!;
        var result = await ordered.ToListAsync();

        var ages = result.Select(e => e.Age).ToList();
        Assert.Equal(ages.OrderBy(a => a), ages);
    }

    [Fact]
    public async Task OrderByProperty_StringAscending_SortsAscending()
    {
        var query = Fixture.Context.Entities.AsQueryable();
        var ordered = query.OrderByProperty("Name")!;
        var result = await ordered.ToListAsync();

        var names = result.Select(e => e.Name).ToList();
        Assert.Equal(names.OrderBy(n => n, StringComparer.Ordinal), names);
    }

    // --- OrderByPropertyDescending ----------------------------------------

    [Fact]
    public async Task OrderByPropertyDescending_SortsDescending()
    {
        var query = Fixture.Context.Entities.AsQueryable();
        var ordered = query.OrderByPropertyDescending("Age")!;
        var result = await ordered.ToListAsync();

        var ages = result.Select(e => e.Age).ToList();
        Assert.Equal(ages.OrderByDescending(a => a), ages);
    }

    // --- null return on missing property -----------------------------------

    [Fact]
    public void OrderByProperty_NonExistentProperty_ReturnsNull()
    {
        var query = Fixture.Context.Entities.AsQueryable();
        Assert.Null(query.OrderByProperty("NoSuchProperty"));
    }

    [Fact]
    public void OrderByPropertyDescending_NonExistentProperty_ReturnsNull()
    {
        var query = Fixture.Context.Entities.AsQueryable();
        Assert.Null(query.OrderByPropertyDescending("NoSuchProperty"));
    }

    // --- via Repository.GetAll with orderBy --------------------------------

    [Fact]
    public async Task GetAll_WithOrderByAsc_ReturnsSortedAscending()
    {
        var page = new PagingQuery { PageNumber = 1, PageSize = 10 };
        var result = await Fixture.Repository.GetAll(page, orderBy: "Age", asc: true);

        var ages = result.Select(e => e.Age).ToList();
        Assert.Equal(ages.OrderBy(a => a), ages);
    }

    [Fact]
    public async Task GetAll_WithOrderByDesc_ReturnsSortedDescending()
    {
        var page = new PagingQuery { PageNumber = 1, PageSize = 10 };
        var result = await Fixture.Repository.GetAll(page, orderBy: "Age", asc: false);

        var ages = result.Select(e => e.Age).ToList();
        Assert.Equal(ages.OrderByDescending(a => a), ages);
    }

    [Fact]
    public async Task GetAll_UnknownOrderBy_FallsBackToDefaultSortProperty()
    {
        var page = new PagingQuery { PageNumber = 1, PageSize = 10 };
        var result = await Fixture.Repository.GetAll(page, orderBy: "NoSuchField");

        // Unknown property doesn't throw — returns all records
        Assert.Equal(5, result.TotalCount);
    }

    // --- GetDefaultSortProperty fallback chain ----------------------------

    [Fact]
    public void GetDefaultSortProperty_EntityWithAttribute_ReturnsAttributeProperty()
    {
        // TestEntity has [DefaultSortProperty] on CreatedAt
        Assert.Equal("CreatedAt", Fixture.Repository.GetDefaultSortProperty());
    }
}

[Collection("SqlServer")]
public class OrderByTests_SqlServer : OrderByTestsBase
{
    public OrderByTests_SqlServer(SqlServerFixture fixture) : base(fixture) { }
}

[Collection("PostgreSql")]
public class OrderByTests_PostgreSql : OrderByTestsBase
{
    public OrderByTests_PostgreSql(PostgreSqlFixture fixture) : base(fixture) { }
}

[Collection("MySql")]
public class OrderByTests_MySql : OrderByTestsBase
{
    public OrderByTests_MySql(MySqlFixture fixture) : base(fixture) { }
}
