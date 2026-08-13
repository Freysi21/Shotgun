using Microsoft.Extensions.DependencyInjection;
using TEST.Shotgun.API.Domain;
using TEST.Shotgun.API.Fixtures;
using TEST.Shotgun.API.Infrastructure;
using Xunit;

namespace TEST.Shotgun.API.Tests.E2E;

// Worked example: exercises TestEntityController (Controllers/TestEntityController.cs) — a
// plain Shotgun<TestEntity, TestRepository, int> subclass — over real HTTP against each
// supported database provider.
public abstract class TestEntityE2ETestsBase : ShotgunControllerE2ETestsBase<TestEntity, int>
{
    private readonly DatabaseFixture _fixture;
    protected TestEntityE2ETestsBase(DatabaseFixture fixture) => _fixture = fixture;

    protected override string RoutePrefix => "api/TestEntity";

    protected override HttpClient CreateClient()
    {
        var factory = new ShotgunWebApplicationFactory(services =>
        {
            services.AddSingleton(_fixture.Options);
            services.AddScoped<TestDbContext>();
            services.AddScoped<TestRepository>();
        });
        return factory.CreateClient();
    }

    protected override ShotgunE2EOptions<TestEntity, int> Options => new()
    {
        CreateRandom = () =>
        {
            var marker = Guid.NewGuid().ToString("N");
            return new TestEntity
            {
                Name = $"E2E-{marker}",
                Description = "created by e2e test",
                Age = Random.Shared.Next(18, 80),
                Score = Random.Shared.Next(0, 100_000),
                Rating = (short)Random.Shared.Next(1, 5),
                IsActive = true,
                ExternalId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
            };
        },
        ApplyRandomUpdate = entity =>
        {
            entity.Name = $"{entity.Name}-updated";
            entity.Age += 1;
        },
        BuildSearchFilter = entity => new Dictionary<string, string[]>
        {
            ["Name"] = new[] { entity.Name },
        },
    };
}

[Collection("SqlServer")]
public class TestEntityE2ETests_SqlServer : TestEntityE2ETestsBase
{
    public TestEntityE2ETests_SqlServer(SqlServerFixture fixture) : base(fixture) { }
}

[Collection("PostgreSql")]
public class TestEntityE2ETests_PostgreSql : TestEntityE2ETestsBase
{
    public TestEntityE2ETests_PostgreSql(PostgreSqlFixture fixture) : base(fixture) { }
}

[Collection("MySql")]
public class TestEntityE2ETests_MySql : TestEntityE2ETestsBase
{
    public TestEntityE2ETests_MySql(MySqlFixture fixture) : base(fixture) { }
}
