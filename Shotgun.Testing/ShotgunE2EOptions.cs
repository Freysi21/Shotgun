using Shotgun.Entity;

namespace Shotgun.Testing;

// Per-entity configuration the generic E2E harness needs. Kept intentionally small:
// reflection could auto-generate random entities, but breaks on required FKs/navigation
// properties and can't produce a meaningful search filter on its own.
public class ShotgunE2EOptions<TEntity, TId> where TEntity : IEntity<TId>
{
    public required Func<TEntity> CreateRandom { get; init; }
    public required Action<TEntity> ApplyRandomUpdate { get; init; }
    public required Func<TEntity, Dictionary<string, string[]>> BuildSearchFilter { get; init; }
}
