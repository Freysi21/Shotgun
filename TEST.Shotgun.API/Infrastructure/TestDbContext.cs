using Microsoft.EntityFrameworkCore;
using TEST.Shotgun.API.Domain;

namespace TEST.Shotgun.API.Infrastructure;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TestEntity> Entities { get; set; } = null!;
    public DbSet<CategoryEntity> Categories { get; set; } = null!;
    public DbSet<TagEntity> Tags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>()
            .HasMany(e => e.Tags)
            .WithOne(t => t.TestEntity)
            .HasForeignKey(t => t.TestEntityId);

        modelBuilder.Entity<TestEntity>()
            .HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .IsRequired(false);
    }
}
