using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WUIAM.Models
{
    public class WUIAMDbContextFactory : IDesignTimeDbContextFactory<WUIAMDbContext>
    {
        public WUIAMDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("DefaultConnection is required for EF design-time operations.");

            var optionsBuilder = new DbContextOptionsBuilder<WUIAMDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new WUIAMDbContext(optionsBuilder.Options);
        }
    }
}
