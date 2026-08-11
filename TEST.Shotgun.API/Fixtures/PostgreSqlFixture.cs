using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TEST.Shotgun.API.Infrastructure;
using Xunit;

namespace TEST.Shotgun.API.Fixtures;

public class PostgreSqlFixture : DatabaseFixture
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder().Build();

    protected override async Task<DbContextOptions<TestDbContext>> GetOptionsAsync()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        await _container.StartAsync();
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("PostgreSql")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture> { }
