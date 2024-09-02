using FluentAssertions.Common;
using InnoShop.Services.AuthAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InnoShop.TestAuthAPI.IntegrationTests
{
    internal class AuthAPIWebApplicationFactory:WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                var connString = GetConnectionString();
                services.AddSqlServer<AppDbContext>(connString);

                var dbContext=CreateDbContext(services);
                dbContext.Database.EnsureDeleted();
            });

        }

        private static string? GetConnectionString()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<AuthAPIWebApplicationFactory>()
                .Build();

            var connString = configuration.GetConnectionString("InnoShop_Auth");
            return connString;
        }

        private static AppDbContext CreateDbContext(IServiceCollection services)
        {
            var serviceProvider= services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            var dbContext=scope.ServiceProvider.GetService<AppDbContext>();
            return dbContext;
        }
    }
}
