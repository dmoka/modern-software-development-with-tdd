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
using VerticalSlicingArchitecture.Tests.Shared;

namespace VerticalSlicingArchitecture.Tests.Features.Product;

public class CreateProductIntegrationTests
{

    [Test]
    public async Task CreateProduct_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();

        var command = new CreateProduct.Command
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await testServer.Client().PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Test]
    public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();

        var command = new CreateProduct.Command
        {
            Name = "", // Invalid: empty name
            Description = "Test Description",
            Price = -1, // Invalid: negative price
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await testServer.Client().PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
} 