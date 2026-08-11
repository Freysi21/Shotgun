using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using TEST.Shotgun.API.Infrastructure;
using Xunit;

namespace TEST.Shotgun.API.Fixtures;

public class SqlServerFixture : DatabaseFixture
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();

    protected override async Task<DbContextOptions<TestDbContext>> GetOptionsAsync()
    {
        await _container.StartAsync();
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("SqlServer")]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture> { }
