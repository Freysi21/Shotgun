using Shotgun.Entity;

namespace TEST.Shotgun.API.Domain;

// Covers every type branch in Search.cs (string, int/int?, long/long?, short/short?,
// bool/bool?, Guid/Guid?) plus DateTime/DateTime? for Range.cs.
public class TestEntity : IEntity<int>
{
    public override int Id { get; set; }

    // string
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // int / int?
    public int Age { get; set; }
    public int? NullableAge { get; set; }

    // long / long?
    public long Score { get; set; }
    public long? NullableScore { get; set; }

    // short / short?
    public short Rating { get; set; }
    public short? NullableRating { get; set; }

    // bool / bool?
    public bool IsActive { get; set; }
    public bool? IsVerified { get; set; }

    // Guid / Guid?
    public Guid ExternalId { get; set; }
    public Guid? NullableExternalId { get; set; }

    // DateTime / DateTime? — used by Range.cs; [DefaultSortProperty] drives GetDefaultSortProperty()
    [DefaultSortPropertyAttribute]
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // navigation — one-to-one
    public int? CategoryId { get; set; }
    [SingleNavigationPropertyAttribute]
    public CategoryEntity? Category { get; set; }

    // navigation — one-to-many
    [NavigationPropertyAttribute]
    public ICollection<TagEntity>? Tags { get; set; }
}
