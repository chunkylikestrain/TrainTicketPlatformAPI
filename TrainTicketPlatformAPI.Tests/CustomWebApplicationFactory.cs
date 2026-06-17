using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.TestHost;
using TrainTicketPlatformAPI.Data;

namespace TrainTicketPlatformAPI.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"ApiIntegrationTests-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.UseSetting("Jwt:Key", "0123456789ABCDEF0123456789ABCDEF");
            builder.UseSetting("Jwt:Issuer", "TestIssuer");
            builder.UseSetting("Jwt:Audience", "TestAudience");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "0123456789ABCDEF0123456789ABCDEF",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience",
                    ["SeedData:UseDevelopmentSeedData"] = "true",
                    ["SeedData:AdminPassword"] = DevelopmentSeedData.DefaultPassword,
                    ["SeedData:PassengerPassword"] = DevelopmentSeedData.DefaultPassword
                });
            });

            builder.ConfigureTestServices(services =>
            {
                var descriptors = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType == typeof(DbContextOptions<TrainTicketDbContext>) ||
                        d.ServiceType.Name.Contains("IDbContextOptionsConfiguration", StringComparison.OrdinalIgnoreCase) ||
                        d.ServiceType.FullName?.Contains("Microsoft.EntityFrameworkCore.SqlServer", StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();

                foreach (var descriptor in descriptors)
                    services.Remove(descriptor);

                services.AddDbContext<TrainTicketDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));

                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "TrainTicketPlatformAPI.Tests.Keys")));

                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TrainTicketDbContext>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                DevelopmentSeedData.SeedAsync(db, configuration).GetAwaiter().GetResult();
            });
        }
    }
}
