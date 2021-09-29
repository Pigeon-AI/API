using Microsoft.EntityFrameworkCore;

namespace PigeonAPI;

public class DatabaseAccess : DbContext
{
    /// <summary>
    /// Database table for images
    /// </summary>
    public DbSet<ImageFile>? Images { get; set; }

    /// <summary>
    /// Path of the database, temporary for now
    /// </summary>
    public string DbPath { get; } = Path.GetTempFileName();

    /// <summary>
    /// Default constructor
    /// </summary>
    public DatabaseAccess() {

    }

    /// <summary>
    /// Configuration function explaining how to setup db
    /// </summary>
    /// <param name="options">Options variable to add onto</param>
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}
