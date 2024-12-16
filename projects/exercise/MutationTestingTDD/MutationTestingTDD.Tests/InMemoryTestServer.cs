using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MutationTestingTDD.Data;
using MutationTestingTDD.Domain;

namespace MutationTestingTDD.Tests;

public class InMemoryTestServer : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly WarehousingDbContext _dbContext;

    public InMemoryTestServer()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove all DbContext and SQL Server related services
                    var descriptors = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<WarehousingDbContext>)
                            || d.ServiceType == typeof(WarehousingDbContext)
                            || (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                        .ToList();

                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    // Remove all database provider services
                    var dbProviderServices = services
                        .Where(x => x.ServiceType.Namespace?.Contains("EntityFrameworkCore") == true)
                        .ToList();

                    foreach (var descriptor in dbProviderServices)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database with Scoped lifetime
                    services.AddDbContext<WarehousingDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                    }, ServiceLifetime.Singleton);

                    // Register required services
                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    services.AddScoped<IProductsSearcher, ProductsSearcher>();
                    services.AddControllers();

                    services.Configure<HttpsRedirectionOptions>(options =>
                    {
                        options.HttpsPort = 443;
                    });
                });

                //Restrict EF Core logs
                builder.ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Warning);
                    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                });
            });

        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<WarehousingDbContext>();
        _client = _factory.CreateClient();
    }

    public HttpClient Client() => _client;

    public WarehousingDbContext DbContext() => _dbContext;

    public void Dispose()
    {
        _client.Dispose();
        _scope.Dispose();
        _factory.Dispose();
    }

    public async Task AddProductsToDbContext(params Product[] products)
    {
        foreach (var product in products)
        {
            _dbContext.Products.Add(product);
        }

        await _dbContext.SaveChangesAsync();
    }
}
