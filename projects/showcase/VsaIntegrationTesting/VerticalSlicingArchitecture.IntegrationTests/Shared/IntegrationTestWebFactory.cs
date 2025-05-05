using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using VerticalSlicingArchitecture.Database;
using Xunit;

namespace VerticalSlicingArchitecture.IntegrationTests.Shared;

public class IntegrationTestWebFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-CU3-ubuntu-20.04")
        .WithPassword("P@ssw0rd!")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithEnvironment("MSSQL_PID", "Developer")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var dbContextServiceDescriptor = services.SingleOrDefault(s =>
                s.ServiceType == typeof(DbContextOptions<WarehousingDbContext>));

            if (dbContextServiceDescriptor is not null)
            {
                // Remove the actual dbcontext registration.
                services.Remove(dbContextServiceDescriptor);
            }

            // Register dbcontext with container connection string
            services.AddDbContext<WarehousingDbContext>(options =>
            {
                string containerConnectionString = _container.GetConnectionString();
                options.UseSqlServer(containerConnectionString);
            });
        });
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _container.StartAsync();

            var serviceProvider = Services.CreateScope().ServiceProvider;

            // Get the db context and apply migrations
           using var dbContext = serviceProvider.GetRequiredService<WarehousingDbContext>();
            await dbContext.Database.MigrateAsync();

            // Optional: Add logging to verify connection string
            var connectionString = _container.GetConnectionString();
            Console.WriteLine($"Container Connection String: {connectionString}");

            // Optional: Verify database connection
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Container initialization failed: {ex}");
            throw;
        }
    }

    async Task IAsyncLifetime.DisposeAsync() 
    {
        await _container.StopAsync();
    }
}