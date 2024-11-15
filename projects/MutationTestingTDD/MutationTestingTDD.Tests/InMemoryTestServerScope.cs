using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MutationTestingTDD.Data;
using MutationTestingTDD.Domain;
using MutationTestingTDD.Tests.Application.Controllers;

namespace MutationTestingTDD.Tests
{
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using System.Net.Http;
    using System.Threading.Tasks;

    namespace MutationTestingTDD.Tests
    {
        using Microsoft.AspNetCore.Mvc.Testing;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.EntityFrameworkCore;
        using System.Net.Http;
        using System.Threading.Tasks;

        namespace MutationTestingTDD.Tests
        {
            public class InMemoryTestServerScope : IDisposable
            {
                private readonly WebApplicationFactory<Program> _factory;
                public HttpClient Client { get; }
                public WarehouseDbContext WarehouseDbContext { get; private set; }

                public InMemoryTestServerScope()
                    : this(new Mock<IProductService>()) // Default to using a mocked service if no specific mock is provided
                {
                }

                public InMemoryTestServerScope(Mock<IProductService> mockProductService)
                {
                    _factory = new WebApplicationFactory<Program>()
                        .WithWebHostBuilder(builder =>
                        {
                            builder.ConfigureServices(services =>
                            {
                                var serviceProvider = new ServiceCollection()
                                    .AddEntityFrameworkInMemoryDatabase()
                                    .BuildServiceProvider();

                                services.AddDbContext<WarehouseDbContext>(options =>
                                {
                                    options.UseInMemoryDatabase("InMemoryTestDb");
                                    options.UseInternalServiceProvider(serviceProvider);
                                });

                                // Use the provided mock service
                                services.AddScoped<IProductService>(provider => mockProductService.Object);
                            });
                        });

                    Client = _factory.CreateClient();
                    var scope = _factory.Services.CreateScope();
                    WarehouseDbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
                }
                public async Task AddProductsToDbContext(params Product[] products)
                {
                    foreach (var product in products)
                    {
                        WarehouseDbContext.Add(product);
                    }
                    await WarehouseDbContext.SaveChangesAsync();
                }

                public async Task AddStockLevel(StockLevel stockLevel)
                {
                    WarehouseDbContext.Add(stockLevel);
                    await WarehouseDbContext.SaveChangesAsync();
                }

                public T GetService<T>()
                {
                    var scope = _factory.Services.CreateScope();
                    return scope.ServiceProvider.GetRequiredService<T>();
                }

                public void Dispose()
                {
                    Client?.Dispose();
                    _factory?.Dispose();
                    // Explicitly dispose of the DbContext or reset it if necessary
                    if (WarehouseDbContext != null)
                    {
                        WarehouseDbContext.Database.EnsureDeleted();
                        WarehouseDbContext.Dispose();
                    }
                }
            }
        }
    }
}
