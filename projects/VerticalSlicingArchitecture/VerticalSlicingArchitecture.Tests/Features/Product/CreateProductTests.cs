using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Features.Product;
using VerticalSlicingArchitecture.Tests.Asserters;
using VerticalSlicingArchitecture.Tests.Shared;

namespace VerticalSlicingArchitecture.Tests.Features.Product;

public class CreateProductTests
{

    [Test]
    public async Task CreateProductShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();

        var command = new CreateProduct.Command
        {
            Name = "", // Invalid: empty name
            Description = "Test Description",
            Price = 100, // Invalid: negative price
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await testServer.Client().PostAsync("/api/products", content);

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "CreateArticle.Validation",
            description = "'Name' must not be empty."
        });
    }

    [Test]
    public async Task CreateProductShouldFail_WhenInvalidPrice()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();

        var command = new CreateProduct.Command
        {
            Name = "WebCam",
            Description = "Test Description",
            Price = -1,
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await testServer.Client().PostAsync("/api/products", content);

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "CreateArticle.Validation",
            description = "'Price' must be greater than '0'."
        });
    }

    [Test]
    public async Task CreateProductShouldFail_WhenNoDescription()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();

        var command = new CreateProduct.Command
        {
            Name = "WebCam",
            Description = "",
            Price = 100,
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await testServer.Client().PostAsync("/api/products", content);

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "CreateArticle.Validation",
            description = "'Description' must not be empty."
        });
    }

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
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Created);
        var product = await testServer.DbContext().Products.FirstOrDefaultAsync();
        product.Should().NotBeNull();
        product.Name.Should().Be(command.Name);
        product.Description.Should().Be(command.Description);
        product.Price.Should().Be(command.Price);

        var stocklevel = await testServer.DbContext().StockLevels.FirstOrDefaultAsync();
        stocklevel.Should().NotBeNull();
        stocklevel.ProductId.Should().Be(product.Id);
        stocklevel.Quantity.Should().Be(command.InitialStock);
        stocklevel.QualityStatus.Should().Be(QualityStatus.Available);

        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            id = product.Id
        });
    }

    [Test]
    public async Task CreateProductShouldFail_WhenInitialQualityBiggerThanMaxStockLevel()
    {
        // Arrange
        using var testServer = new InMemoryTestServer();

        var command = new CreateProduct.Command
        {
            Name = "WebCam", // Invalid: empty name
            Description = "The new generational web cam",
            Price = 100, // Invalid: negative price
            InitialStock = 51
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await testServer.Client().PostAsync("/api/products", content);

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            code = "CreateArticle.Validation",
            description = "The quantity can not be more than max stock level"
        });
    }

} 