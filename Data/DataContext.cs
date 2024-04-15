using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;
public class DataContext : DbContext
{
    public DbSet<BoxEntity> Boxes { get; set; }
    public DbSet<ItemEntity> Items { get; set; }

    public DataContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // for debugging
        //options
        //    .LogTo(Console.WriteLine)
        //    .EnableSensitiveDataLogging()
        //    .EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: Move to entity configurer extensions
        modelBuilder.Entity<BoxEntity>()
                .HasKey(b => b.Id);
        modelBuilder.Entity<BoxEntity>()
            .HasAlternateKey(b => b.Identifier);

        modelBuilder.Entity<BoxEntity>()
            .HasMany(b => b.Items)
            .WithOne(i => i.Box)
            .HasForeignKey(i => i.BoxId)
            .HasPrincipalKey(b => b.Id);

        modelBuilder.Entity<ItemEntity>()
                .HasKey(i => i.Id);

        base.OnModelCreating(modelBuilder);
    }

}
