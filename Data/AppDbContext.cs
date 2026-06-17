using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Sale> Sales => Set<Sale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable("Sales");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.Location).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }
}
