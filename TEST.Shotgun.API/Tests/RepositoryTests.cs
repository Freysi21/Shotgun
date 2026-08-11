using Microsoft.EntityFrameworkCore;
using Shotgun.Helpers;
using TEST.Shotgun.API.Domain;
using TEST.Shotgun.API.Fixtures;
using TEST.Shotgun.API.Infrastructure;
using Xunit;

namespace TEST.Shotgun.API.Tests;

public abstract class RepositoryTestsBase : IAsyncLifetime
{
    protected readonly DatabaseFixture Fixture;
    protected RepositoryTestsBase(DatabaseFixture fixture) => Fixture = fixture;

    // Entities created by individual tests are tracked here for cleanup
    private readonly List<int> _createdIds = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var id in _createdIds)
        {
            try { await Fixture.Repository.Delete(id); } catch { /* already gone */ }
        }
        _createdIds.Clear();
    }

    private async Task<TestEntity> AddTracked(TestEntity entity)
    {
        var added = await Fixture.Repository.Add(entity);
        _createdIds.Add(added.Id);
        return added;
    }

    // --- GetAll -----------------------------------------------------------

    [Fact]
    public async Task GetAll_ReturnsAllSeededEntities()
    {
        var result = await Fixture.Repository.GetAll();
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetAll_WithPaging_ReturnsCorrectPage()
    {
        var page = new PagingQuery { PageNumber = 1, PageSize = 2 };
        var result = await Fixture.Repository.GetAll(page);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.TotalPages);
        Assert.False(result.HasPrevious);
        Assert.True(result.HasNext);
    }

    [Fact]
    public async Task GetAll_LastPage_HasNextFalse()
    {
        var page = new PagingQuery { PageNumber = 3, PageSize = 2 };
        var result = await Fixture.Repository.GetAll(page);

        Assert.Single(result); // 5 total, page 3 of 2 = 1 item
        Assert.True(result.HasPrevious);
        Assert.False(result.HasNext);
    }

    [Fact]
    public async Task GetAll_DefaultSort_OrdersByCreatedAtDescending()
    {
        var page = new PagingQuery { PageNumber = 1, PageSize = 10 };
        var result = await Fixture.Repository.GetAll(page);

        var dates = result.Select(e => e.CreatedAt).ToList();
        Assert.Equal(dates.OrderByDescending(d => d), dates);
    }

    // --- Get --------------------------------------------------------------

    [Fact]
    public async Task Get_ExistingId_ReturnsEntity()
    {
        var id = Fixture.SeededEntities[0].Id;
        var result = await Fixture.Repository.Get(id);
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task Get_NonExistentId_ReturnsNull()
    {
        var result = await Fixture.Repository.Get(int.MaxValue);
        Assert.Null(result);
    }

    // --- Add / Put / Delete -----------------------------------------------

    [Fact]
    public async Task Add_NewEntity_PersistsToDatabase()
    {
        var entity = new TestEntity
        {
            Name = "Test Add", Age = 99, Score = 9999, Rating = 1,
            IsActive = false, ExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
        };
        var added = await AddTracked(entity);

        Assert.True(added.Id > 0);
        var fetched = await Fixture.Repository.Get(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Test Add", fetched!.Name);
    }

    [Fact]
    public async Task Put_ExistingEntity_UpdatesInDatabase()
    {
        var entity = new TestEntity
        {
            Name = "Before Update", Age = 10, Score = 10, Rating = 1,
            IsActive = false, ExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
        };
        var added = await AddTracked(entity);

        added.Name = "After Update";
        await Fixture.Repository.Put(added);

        var fetched = await Fixture.Repository.Get(added.Id);
        Assert.Equal("After Update", fetched!.Name);
    }

    [Fact]
    public async Task Delete_ExistingId_RemovesEntity()
    {
        var entity = new TestEntity
        {
            Name = "To Delete", Age = 1, Score = 1, Rating = 1,
            IsActive = false, ExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
        };
        var added = await Fixture.Repository.Add(entity);

        var deleted = await Fixture.Repository.Delete(added.Id);
        Assert.NotNull(deleted);

        var fetched = await Fixture.Repository.Get(added.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task Delete_NonExistentId_ReturnsNull()
    {
        var result = await Fixture.Repository.Delete(int.MaxValue);
        Assert.Null(result);
    }

    // --- GetWithDetails ---------------------------------------------------

    [Fact]
    public async Task GetWithDetails_LoadsNavigationProperties()
    {
        var id = Fixture.SeededEntities[0].Id; // Alice has Category + Tags
        var result = await Fixture.Repository.GetWithDetails(id);

        Assert.NotNull(result);
        Assert.NotNull(result!.Category);
        Assert.NotNull(result.Tags);
        Assert.True(result.Tags!.Count >= 2); // "dotnet", "csharp"
    }

    // --- Search -----------------------------------------------------------

    [Fact]
    public async Task Search_WithoutPaging_ReturnsFilteredList()
    {
        var dict = new Dictionary<string, string[]> { { "IsActive", new[] { "true" } } };
        var result = await Fixture.Repository.Search(dict);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Search_WithPaging_ReturnsPagedResult()
    {
        var page = new PagingQuery { PageNumber = 1, PageSize = 2 };
        var dict = new Dictionary<string, string[]> { { "IsActive", new[] { "true" } } };
        // Pass orderBy explicitly to resolve ambiguity between the two Search overloads
        var result = await Fixture.Repository.Search(page, dict, orderBy: null, asc: false);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Count);
    }

    // --- searchIncludes ---------------------------------------------------

    [Fact]
    public async Task Search_WithSearchIncludes_EagerLoadsRelations()
    {
        var repoWithIncludes = new TestRepository(
            Fixture.Context, new[] { "Category", "Tags" });

        var dict = new Dictionary<string, string[]> { { "IsActive", new[] { "true" } } };
        var result = await repoWithIncludes.Search(dict);

        // Every returned entity should have Category loaded (or null if none set)
        // Alice and Alice Smith have categories
        var withCategory = result.Where(e => e.CategoryId != null).ToList();
        Assert.All(withCategory, e => Assert.NotNull(e.Category));
    }

    // --- PagedList metadata -----------------------------------------------

    [Fact]
    public async Task PagedList_SinglePage_HasPreviousAndNextBothFalse()
    {
        var page = new PagingQuery { PageNumber = 1, PageSize = 50 }; // max 50
        var result = await Fixture.Repository.GetAll(page);

        Assert.False(result.HasPrevious);
        Assert.False(result.HasNext);
        Assert.Equal(1, result.TotalPages);
    }

    // --- GetDefaultSortProperty fallback ----------------------------------

    [Fact]
    public void GetDefaultSortProperty_UsesAttributeBeforeDateTimeFallback()
    {
        // TestEntity has [DefaultSortProperty] on CreatedAt — attribute should win
        Assert.Equal("CreatedAt", Fixture.Repository.GetDefaultSortProperty());
    }
}

[Collection("SqlServer")]
public class RepositoryTests_SqlServer : RepositoryTestsBase
{
    public RepositoryTests_SqlServer(SqlServerFixture fixture) : base(fixture) { }
}

[Collection("PostgreSql")]
public class RepositoryTests_PostgreSql : RepositoryTestsBase
{
    public RepositoryTests_PostgreSql(PostgreSqlFixture fixture) : base(fixture) { }
}

[Collection("MySql")]
public class RepositoryTests_MySql : RepositoryTestsBase
{
    public RepositoryTests_MySql(MySqlFixture fixture) : base(fixture) { }
}
