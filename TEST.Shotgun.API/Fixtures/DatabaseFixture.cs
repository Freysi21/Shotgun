using Microsoft.EntityFrameworkCore;
using TEST.Shotgun.API.Domain;
using TEST.Shotgun.API.Infrastructure;
using Xunit;

namespace TEST.Shotgun.API.Fixtures;

// Shared fixture: one Docker container per provider, reused across all test classes
// in the same xunit collection. Seeded once at collection startup.
public abstract class DatabaseFixture : IAsyncLifetime
{
    public TestDbContext Context { get; private set; } = null!;
    public TestRepository Repository { get; private set; } = null!;
    public List<TestEntity> SeededEntities { get; private set; } = new();
    public List<CategoryEntity> SeededCategories { get; private set; } = new();

    protected abstract Task<DbContextOptions<TestDbContext>> GetOptionsAsync();

    public async Task InitializeAsync()
    {
        var options = await GetOptionsAsync();
        Context = new TestDbContext(options);
        await Context.Database.EnsureCreatedAsync();

        var categories = new[]
        {
            new CategoryEntity { Name = "Engineering" },
            new CategoryEntity { Name = "Marketing" },
        };
        await Context.Categories.AddRangeAsync(categories);
        await Context.SaveChangesAsync();

        SeededCategories = categories.ToList();

        var entities = SeedData.CreateEntities(new[] { categories[0].Id, categories[1].Id });
        await Context.Entities.AddRangeAsync(entities);
        await Context.SaveChangesAsync();

        SeededEntities = await Context.Entities.ToListAsync();

        var tags = new[]
        {
            new TagEntity { Label = "dotnet",    TestEntityId = SeededEntities[0].Id },
            new TagEntity { Label = "csharp",    TestEntityId = SeededEntities[0].Id },
            new TagEntity { Label = "dotnet",    TestEntityId = SeededEntities[2].Id },
            new TagEntity { Label = "marketing", TestEntityId = SeededEntities[1].Id },
        };
        await Context.Tags.AddRangeAsync(tags);
        await Context.SaveChangesAsync();

        Repository = new TestRepository(Context);
    }

    public virtual async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }
}
