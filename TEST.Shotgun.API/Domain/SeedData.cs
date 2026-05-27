namespace TEST.Shotgun.API.Domain;

public static class SeedData
{
    public static readonly Guid Guid1 = new("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid Guid2 = new("aaaaaaaa-0000-0000-0000-000000000002");
    public static readonly Guid Guid3 = new("aaaaaaaa-0000-0000-0000-000000000003");
    public static readonly Guid Guid4 = new("aaaaaaaa-0000-0000-0000-000000000004");
    public static readonly Guid Guid5 = new("aaaaaaaa-0000-0000-0000-000000000005");

    public static readonly Guid NullableGuid1 = new("bbbbbbbb-0000-0000-0000-000000000001");
    public static readonly Guid NullableGuid3 = new("bbbbbbbb-0000-0000-0000-000000000003");

    // Dates spread across 2024 to allow range-filter testing
    public static TestEntity[] CreateEntities(int[] categoryIds) => new[]
    {
        new TestEntity
        {
            Name = "Alice", Description = "senior developer",
            Age = 30, NullableAge = 25,
            Score = 1000L, NullableScore = 500L,
            Rating = 5, NullableRating = 4,
            IsActive = true, IsVerified = true,
            ExternalId = Guid1, NullableExternalId = NullableGuid1,
            CreatedAt = new DateTime(2024, 1, 15), UpdatedAt = new DateTime(2024, 2, 1),
            CategoryId = categoryIds[0],
        },
        new TestEntity
        {
            Name = "Bob", Description = null,
            Age = 25, NullableAge = null,
            Score = 2000L, NullableScore = null,
            Rating = 3, NullableRating = null,
            IsActive = false, IsVerified = null,
            ExternalId = Guid2, NullableExternalId = null,
            CreatedAt = new DateTime(2024, 3, 10), UpdatedAt = null,
            CategoryId = categoryIds[1],
        },
        new TestEntity
        {
            Name = "Charlie", Description = "alice lookalike",
            Age = 35, NullableAge = 35,
            Score = 500L, NullableScore = 750L,
            Rating = 4, NullableRating = 5,
            IsActive = true, IsVerified = false,
            ExternalId = Guid3, NullableExternalId = NullableGuid3,
            CreatedAt = new DateTime(2024, 6, 20), UpdatedAt = new DateTime(2024, 7, 1),
            CategoryId = null,
        },
        new TestEntity
        {
            Name = "Alice Smith", Description = "product manager",
            Age = 28, NullableAge = 28,
            Score = 1500L, NullableScore = 1200L,
            Rating = 5, NullableRating = 5,
            IsActive = true, IsVerified = true,
            ExternalId = Guid4, NullableExternalId = null,
            CreatedAt = new DateTime(2024, 2, 5), UpdatedAt = new DateTime(2024, 2, 10),
            CategoryId = categoryIds[0],
        },
        new TestEntity
        {
            Name = "Dave", Description = "intern",
            Age = 25, NullableAge = null,
            Score = 750L, NullableScore = null,
            Rating = 2, NullableRating = null,
            IsActive = false, IsVerified = false,
            ExternalId = Guid5, NullableExternalId = null,
            CreatedAt = new DateTime(2024, 4, 1), UpdatedAt = null,
            CategoryId = categoryIds[1],
        },
    };
}
