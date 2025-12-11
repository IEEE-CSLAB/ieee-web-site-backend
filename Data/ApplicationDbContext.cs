using IEEEBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace IEEEBackend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Admin> Admins { get; set; }
    public DbSet<Committee> Committees { get; set; }
    public DbSet<Executive> Executives { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<EventCommittee> EventCommittees { get; set; }
    public DbSet<EventPhoto> EventPhotos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // EventCommittee composite primary key
        modelBuilder.Entity<EventCommittee>()
            .HasKey(ec => new { ec.EventId, ec.CommitteeId });

        // EventCommittee relationships
        modelBuilder.Entity<EventCommittee>()
            .HasOne(ec => ec.Event)
            .WithMany(e => e.EventCommittees)
            .HasForeignKey(ec => ec.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventCommittee>()
            .HasOne(ec => ec.Committee)
            .WithMany(c => c.EventCommittees)
            .HasForeignKey(ec => ec.CommitteeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Executive relationship
        modelBuilder.Entity<Executive>()
            .HasOne(e => e.Committee)
            .WithMany(c => c.Executives)
            .HasForeignKey(e => e.CommitteeId)
            .OnDelete(DeleteBehavior.Cascade);

        // BlogPost relationship
        modelBuilder.Entity<BlogPost>()
            .HasOne(bp => bp.Committee)
            .WithMany(c => c.BlogPosts)
            .HasForeignKey(bp => bp.CommitteeId)
            .OnDelete(DeleteBehavior.Cascade);

        // EventPhoto relationship
        modelBuilder.Entity<EventPhoto>()
            .HasOne(ep => ep.Event)
            .WithMany(e => e.EventPhotos)
            .HasForeignKey(ep => ep.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure timestamps for BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.CreatedAt))
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.UpdatedAt))
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && 
                   (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}

