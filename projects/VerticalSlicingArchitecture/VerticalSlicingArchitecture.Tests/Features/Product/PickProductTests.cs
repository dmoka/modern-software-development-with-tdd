using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Entities;
using VerticalSlicingArchitecture.Features.Product;

namespace VerticalSlicingArchitecture.Tests.Features.Product;

public class PickProductIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IServiceScope _scope;
    private WarehousingDbContext _dbContext;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<WarehousingDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<WarehousingDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    // Register validators and MediatR
                    var assembly = typeof(Program).Assembly;
                    services.AddValidatorsFromAssembly(assembly);
                    services.AddMediatR(config => 
                        config.RegisterServicesFromAssembly(assembly));
                });
            });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<WarehousingDbContext>();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        _scope.Dispose();
        _dbContext.Dispose();
    }

    [SetUp]
    public async Task Setup()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [Test]
    public async Task PickProduct_WithValidData_ShouldReduceStockLevel()
    {
        // Arrange
        var product = new Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
        };
        await _dbContext.Products.AddAsync(product);

        var stockLevel = StockLevel.New(product.Id, 10, DateTime.Now).Value;
        await _dbContext.StockLevels.AddAsync(stockLevel);
        
        await _dbContext.SaveChangesAsync();

        var command = new PickProduct.Command(product.Id, 3);
        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/products/{product.Id}/pick", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newStockLevel = await _dbContext.StockLevels.AsNoTracking()
            .FirstOrDefaultAsync(sl => sl.ProductId == product.Id);

        newStockLevel.Should().NotBeNull();
        newStockLevel!.Quantity.Should().Be(7);
    }
} 