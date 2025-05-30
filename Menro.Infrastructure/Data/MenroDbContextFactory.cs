using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Menro.Infrastructure.Data;

public class MenroDbContextFactory : IDesignTimeDbContextFactory<MenroDbContext>
{
    public MenroDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MenroDbContext>();

        // اتصال به دیتابیس را اینجا تعریف کن (Connection String)
        var connectionString = "Server=.;Database=MenroDb;Trusted_Connection=True;TrustServerCertificate=True;";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new MenroDbContext(optionsBuilder.Options);
    }
}
