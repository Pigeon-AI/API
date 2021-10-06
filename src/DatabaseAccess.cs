using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PigeonAPI.Models;

namespace PigeonAPI;

public class DatabaseAccess : DbContext
{
    /// <summary>
    /// A database table with all items
    /// </summary>
    public DbSet<DatabaseImage> Images { get; set; } = null!;

    /// <summary>
    /// ASP.Net core required: logger
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Default constructor
    /// </summary>
    public DatabaseAccess(ILogger logger)
    {
        this._logger = logger;
    }

    /// <summary>
    /// Tell it to use sqlite for debugging purposes
    /// </summary>
    /// <param name="options"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Get postgres database url
        string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (databaseUrl != null)
        {
            this._logger.LogDebug("Database URL found successfully, using Heroku Postgresql");
            options.UseNpgsql(PostgresConnectionString(databaseUrl));

            this._logger.LogDebug("Engaging Postgresql connection.");
        }
        else
        {
            this._logger.LogInformation("Database URL not found, falling back on Sqlite");

            // Put it in a folder with the executables running this
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db.sqlite3");
            options.UseSqlite($"Data Source={dbPath}");

            this._logger.LogDebug($"Engaging Sqlite connection to path {dbPath}");
        }
    }

    /// <summary>
    /// A function to convert the heroku postgres environment variable to a valid connection string
    /// </summary>
    private static string PostgresConnectionString(string envDatabaseUrl)
        {
            // get heroku environment variable
            string connectionString = envDatabaseUrl;
            connectionString.Replace("//", "");

            char[] delimiterChars = { '/', ':', '@', '?' };
            string[] strConn = connectionString.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

            var builder = new NpgsqlConnectionStringBuilder
            {
                Username = strConn[1],
                Password = strConn[2],
                Host = strConn[3],
                Port = Int32.Parse(strConn[4]),
                Database = strConn[5],
                SslMode = SslMode.Require,
                // Heroku db has an untrusted certificate
                TrustServerCertificate = true,
                Timeout = 1000
            };

            return builder.ToString();
        }
}
