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
using VerticalSlicingArchitecture.Features.Product;

namespace VerticalSlicingArchitecture.Tests.Features.Product;

public class CreateProductIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

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

                    // Register validators
                    var assembly = typeof(Program).Assembly;
                    services.AddValidatorsFromAssembly(assembly);

                    // Register MediatR
                    services.AddMediatR(config => 
                        config.RegisterServicesFromAssembly(assembly));
                });
            });

        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task CreateProduct_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new CreateProduct.Command
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Test]
    public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateProduct.Command
        {
            Name = "", // Invalid: empty name
            Description = "Test Description",
            Price = -1, // Invalid: negative price
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
} 