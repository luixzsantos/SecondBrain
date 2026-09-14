using Microsoft.EntityFrameworkCore;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Infrastructure.Persistence;

public class SecondBrainDbContext(DbContextOptions<SecondBrainDbContext> options) : DbContext(options)
{
    public DbSet<Concept> Concepts => Set<Concept>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Concept>(entity =>
        {
            entity.ToTable("concepts");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Description).HasMaxLength(4000);
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.Property(c => c.UpdatedAt).IsRequired();

            // Nome único (case-insensitive via citext seria ideal; index padrão por ora)
            entity.HasIndex(c => c.Name).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
