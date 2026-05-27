using Shotgun.Entity;

namespace TEST.Shotgun.API.Domain;

public class CategoryEntity : IEntity<int>
{
    public override int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
