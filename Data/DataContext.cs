using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;
public class DataContext : DbContext
{
    public DbSet<BoxEntity> Boxes { get; set; }
    public DbSet<ItemEntity> Items { get; set; }

    private readonly string _dbPath = "";

    // normally i would configure dataContext in Program.cs, but since its just a local db i just put the config here
    public DataContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        _dbPath = Path.Join(path, "boxes.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
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
