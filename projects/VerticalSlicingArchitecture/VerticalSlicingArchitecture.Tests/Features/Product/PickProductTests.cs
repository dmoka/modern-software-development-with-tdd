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
using VerticalSlicingArchitecture.Tests.Shared;

namespace VerticalSlicingArchitecture.Tests.Features.Product;

public class PickProductIntegrationTests
{

    [Test]
    public async Task PickProduct_WithValidData_ShouldReduceStockLevel()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();
        var product = new Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
        };
        await testServer.DbContext().Products.AddAsync(product);
        await testServer.DbContext().SaveChangesAsync();

        var stockLevel = StockLevel.New(product.Id, 10, DateTime.Now).Value;
        await testServer.DbContext().StockLevels.AddAsync(stockLevel);
        
        await testServer.DbContext().SaveChangesAsync();

        var command = new PickProduct.Command(product.Id, 3);
        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var c = testServer.DbContext();
        // Act
        var response = await testServer.Client().PostAsync($"/api/products/{product.Id}/pick", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newStockLevel = await testServer.DbContext().StockLevels.AsNoTracking()
            .FirstOrDefaultAsync(sl => sl.ProductId == product.Id);

        newStockLevel.Should().NotBeNull();
        newStockLevel!.Quantity.Should().Be(7);
    }
} 