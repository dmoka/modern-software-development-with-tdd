using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Features.Product;
using VerticalSlicingArchitecture.Tests.Asserters;
using VerticalSlicingArchitecture.Tests.Shared;

namespace VerticalSlicingArchitecture.Tests.Features.Product;

public class GetProductTests
{

    [Test]
    public async Task ReturnsNoProduct_whenNoProductExists()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();
        var id = Guid.NewGuid();

        // Act
        var response = await testServer.Client().GetAsync($"/api/products/{id}");

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.NotFound);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "GetProduct.NotFound",
            description = $"Product with Id {id} was not found."
        });
    }

    [Test]
    public async Task ReturnsProduct_whenProductExists()
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

        // Act
        var response = await testServer.Client().GetAsync($"/api/products/{product.Id}");

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            id = product.Id,
            name = product.Name,
            description = product.Description,
            price = product.Price
        });
    }
} 