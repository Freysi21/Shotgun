using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;
using TEST.Shotgun.API.Infrastructure;
using Xunit;

namespace TEST.Shotgun.API.Fixtures;

public class MySqlFixture : DatabaseFixture
{
    private readonly MySqlContainer _container = new MySqlBuilder().Build();

    protected override async Task<DbContextOptions<TestDbContext>> GetOptionsAsync()
    {
        await _container.StartAsync();
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseMySql(
                _container.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("MySql")]
public class MySqlCollection : ICollectionFixture<MySqlFixture> { }
