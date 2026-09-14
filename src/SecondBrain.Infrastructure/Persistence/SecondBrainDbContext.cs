using Microsoft.EntityFrameworkCore;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Infrastructure.Persistence;

public class SecondBrainDbContext(DbContextOptions<SecondBrainDbContext> options) : DbContext(options)
{
    public DbSet<Concept> Concepts => Set<Concept>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ConceptNote> ConceptNotes => Set<ConceptNote>();
    public DbSet<ConceptProject> ConceptProjects => Set<ConceptProject>();
    public DbSet<ConceptTag> ConceptTags => Set<ConceptTag>();

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

        modelBuilder.Entity<Note>(entity =>
        {
            entity.ToTable("notes");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Content).IsRequired();
            entity.Property(n => n.CreatedAt).IsRequired();
            entity.Property(n => n.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(t => t.Name).IsUnique();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Description).HasMaxLength(4000);
            entity.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<ConceptNote>(entity =>
        {
            entity.ToTable("concept_notes");
            entity.HasKey(cn => new { cn.ConceptId, cn.NoteId });
            entity.HasOne(cn => cn.Concept).WithMany(c => c.ConceptNotes)
                .HasForeignKey(cn => cn.ConceptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cn => cn.Note).WithMany(n => n.ConceptNotes)
                .HasForeignKey(cn => cn.NoteId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(cn => cn.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<ConceptProject>(entity =>
        {
            entity.ToTable("concept_projects");
            entity.HasKey(cp => new { cp.ConceptId, cp.ProjectId });
            entity.HasOne(cp => cp.Concept).WithMany(c => c.ConceptProjects)
                .HasForeignKey(cp => cp.ConceptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cp => cp.Project).WithMany(p => p.ConceptProjects)
                .HasForeignKey(cp => cp.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(cp => cp.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<ConceptTag>(entity =>
        {
            entity.ToTable("concept_tags");
            entity.HasKey(ct => new { ct.ConceptId, ct.TagId });
            entity.HasOne(ct => ct.Concept).WithMany(c => c.ConceptTags)
                .HasForeignKey(ct => ct.ConceptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ct => ct.Tag).WithMany(t => t.ConceptTags)
                .HasForeignKey(ct => ct.TagId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(ct => ct.CreatedAt).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
