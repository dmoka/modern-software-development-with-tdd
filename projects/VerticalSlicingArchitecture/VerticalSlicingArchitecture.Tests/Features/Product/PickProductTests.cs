using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Features.Product;
using VerticalSlicingArchitecture.Tests.Asserters;
using VerticalSlicingArchitecture.Tests.Shared;

namespace VerticalSlicingArchitecture.Tests.Features.Product;

public class PickProductTests
{

    [Test]
    public async Task PickShouldFail_WhenNoProductIdSpecified()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();
        var command = new PickProduct.Command(Guid.Empty, 3);

        // Act
        var response = await testServer.Client()
            .PostAsync($"/api/products/{Guid.Empty}/pick", JsonPayloadBuilder.Build(command));

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "PickProduct.Validation",
            description = "'Product Id' must not be empty."
        });
    }

    [Test]
    public async Task PickShouldFail_WhenPickCountIsZero()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();
        var product = await CreateProductWith10Units(testServer, 10);

        var command = new PickProduct.Command(product.Id, 0);

        // Act
        var response = await testServer.Client()
            .PostAsync($"/api/products/{product.Id}/pick", JsonPayloadBuilder.Build(command));

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "PickProduct.Validation",
            description = "'Pick Count' must be greater than '0'."
        });
    }


    [Test]
    public async Task PickShouldFail_WhenPickCountIsGreaterThanMaxPickQuantityPerOperation()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();
        var product = await CreateProductWith10Units(testServer, 10);

        var command = new PickProduct.Command(product.Id, 11);

        // Act
        var response = await testServer.Client()
            .PostAsync($"/api/products/{product.Id}/pick", JsonPayloadBuilder.Build(command));

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "PickProduct.ExceedsMaxPick",
            description = "Cannot pick more than 10 items in a single operation"
        });
    }

    [Test]
    public async Task PickShouldFail_WhenStockIsExpired()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();
        var product = await CreateProductWith10Units(testServer, 10);

        var stockLevel = await testServer.DbContext().StockLevels
            .FirstOrDefaultAsync(sl => sl.ProductId == product.Id);

        stockLevel.QualityStatus = QualityStatus.Expired;

        await testServer.DbContext().SaveChangesAsync();

        var command = new PickProduct.Command(product.Id, 3);

        // Act
        var response = await testServer.Client()
            .PostAsync($"/api/products/{product.Id}/pick", JsonPayloadBuilder.Build(command));

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "PickProduct.QualityHold",
            description = "Product is currently under Expired status"
        });
    }

    [Test]
    public async Task PickShouldFail_WhenStockIsDamaged()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();
        var product = await CreateProductWith10Units(testServer, 10);

        var stockLevel = await testServer.DbContext().StockLevels
            .FirstOrDefaultAsync(sl => sl.ProductId == product.Id);

        stockLevel.QualityStatus = QualityStatus.Damaged;

        await testServer.DbContext().SaveChangesAsync();

        var command = new PickProduct.Command(product.Id, 3);

        // Act
        var response = await testServer.Client()
            .PostAsync($"/api/products/{product.Id}/pick", JsonPayloadBuilder.Build(command));

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "PickProduct.QualityHold",
            description = "Product is currently under Damaged status"
        });
    }

    [Test]
    public async Task PickShouldFail_whenPickedMoreThanStockLevel()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();
        var product = await CreateProductWith10Units(testServer, 3);

        var command = new PickProduct.Command(product.Id, 4);

        // Act
        var response = await testServer.Client()
            .PostAsync($"/api/products/{product.Id}/pick", JsonPayloadBuilder.Build(command));

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "PickProduct.InsufficientStock",
            description = "Cannot pick 4 items. Only 3 available"
        });
    }


    [Test]
    public async Task PickProductShouldDecreaseStock()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();
        var product = await CreateProductWith10Units(testServer, 10);

        var command = new PickProduct.Command(product.Id, 3);

        // Act
        var response = await testServer.Client().PostAsync($"/api/products/{product.Id}/pick", JsonPayloadBuilder.Build(command));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newStockLevel = await testServer.DbContext().StockLevels.AsNoTracking()
            .FirstOrDefaultAsync(sl => sl.ProductId == product.Id);

        newStockLevel.Should().NotBeNull();
        newStockLevel!.Quantity.Should().Be(7);
    }

    private static async Task<Entities.Product> CreateProductWith10Units(InMemoryTestServer testServer, int quantity)
    {
        var product = new Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
        };
        await testServer.DbContext().Products.AddAsync(product);
        await testServer.DbContext().SaveChangesAsync();

        var stockLevel = StockLevel.New(product.Id, quantity).Value;
        await testServer.DbContext().StockLevels.AddAsync(stockLevel);
        
        await testServer.DbContext().SaveChangesAsync();
        return product;
    }
} 