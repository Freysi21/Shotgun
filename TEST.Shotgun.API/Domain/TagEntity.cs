using Shotgun.Entity;
using System.Text.Json.Serialization;

namespace TEST.Shotgun.API.Domain;

public class TagEntity : IEntity<int>
{
    public override int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public int TestEntityId { get; set; }

    // JsonIgnore stops Include.GetNavigations from recursing back into TestEntity
    [JsonIgnore]
    [SingleNavigationPropertyAttribute]
    public TestEntity? TestEntity { get; set; }
}
