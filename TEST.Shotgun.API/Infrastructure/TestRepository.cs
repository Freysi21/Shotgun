using Shotgun.Repos;
using TEST.Shotgun.API.Domain;

namespace TEST.Shotgun.API.Infrastructure;

public class TestRepository : EFCoreRepository<TestEntity, TestDbContext, int>
{
    public TestRepository(TestDbContext context) : base(context) { }

    public TestRepository(TestDbContext context, string[] searchIncludes)
        : base(context, searchIncludes) { }

    // Expose protected helper for direct testing
    public new string GetDefaultSortProperty() => base.GetDefaultSortProperty();
}
