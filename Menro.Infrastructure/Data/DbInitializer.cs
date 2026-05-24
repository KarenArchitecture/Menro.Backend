using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Persistence;

public class DbInitializer : IDbInitializer
{
    private readonly MenroDbContext _db;

    private readonly IEnumerable<IDataSeeder> _seeders;

    public DbInitializer(
        MenroDbContext db,
        IEnumerable<IDataSeeder> seeders)
    {
        _db = db;
        _seeders = seeders;
    }

    public async Task InitializeAsync()
    {
        try
        {
            Console.WriteLine(
                "========== Menro DB Init ==========");

            var canConnect =
                await _db.Database.CanConnectAsync();

            if (!canConnect)
                throw new Exception(
                    "Database connection failed.");

            Console.WriteLine(
                "Database connection OK");

            var pendingMigrations =
                await _db.Database
                    .GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                Console.WriteLine(
                    "Applying migrations...");

                await _db.Database.MigrateAsync();

                Console.WriteLine(
                    "Migrations completed.");
            }

            foreach (var seeder in _seeders)
            {
                Console.WriteLine(
                    $"Running: {seeder.GetType().Name}");

                await seeder.SeedAsync();

                Console.WriteLine(
                    $"Completed: {seeder.GetType().Name}");
            }

            Console.WriteLine(
                "Database initialization completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            throw;
        }
    }
}