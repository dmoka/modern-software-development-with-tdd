using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VerticalSlicingArchitecture.Database;
using Wolverine;

namespace VerticalSlicingArchitecture.Tests.Shared;

public class InMemoryTestServer : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly WarehousingDbContext _dbContext;

    public InMemoryTestServer()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    services.RemoveAll(typeof(DbContextOptions<WarehousingDbContext>));

                    // Add in-memory database
                    services.AddDbContext<WarehousingDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"); // Unique DB per instance
                    }, ServiceLifetime.Singleton);
                    

                    // Register validators and MediatR
                    var assembly = typeof(Program).Assembly;
                    services.AddValidatorsFromAssembly(assembly);
                    services.AddMediatR(config =>
                        config.RegisterServicesFromAssembly(assembly));

                    services.Configure<HttpsRedirectionOptions>(options =>
                    {
                        options.HttpsPort = 443;  // Set a dummy port or disable redirection
                    });



                });

                //Restrict EF Core logs
                builder.ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Warning); // This sets the minimum level to Warning, hiding Info and Debug logs
                    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

                    logging.AddFilter("Microsoft.Data", LogLevel.Warning); // MS libraries
                    logging.AddFilter("Microsoft", LogLevel.Warning); // MS libraries
                    logging.AddFilter("System", LogLevel.Warning);    // System libraries
                    logging.AddFilter("Wolverine", LogLevel.Warning); // Wolverine logs
                    logging.AddFilter("VerticalSlicingArchitecture", LogLevel.Warning); // Your app
                    logging.AddFilter("Wolverine.Runtime", LogLevel.Warning);

                    logging.ClearProviders();

                });

                // 🔧 Configure Wolverine for test mode
            });

        _dbContext = _factory.Services.CreateScope()
            .ServiceProvider.GetRequiredService<WarehousingDbContext>();
        _client = _factory.CreateClient();
    }

    public HttpClient Client() => _client;

    public WarehousingDbContext DbContext() => _dbContext;


    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
