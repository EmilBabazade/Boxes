using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;
public class DataContext : DbContext
{
    public DbSet<BoxEntity> Boxes { get; set; }
    public DbSet<ItemEntity> Items { get; set; }

    private readonly string _dbPath = "";

    public DataContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        _dbPath = Path.Join(path, "blogging.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={_dbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoxEntity>()
                .HasKey(b => b.RowId);

        modelBuilder.Entity<ItemEntity>()
                .HasKey(i => i.RowId);

        base.OnModelCreating(modelBuilder);
    }

}
