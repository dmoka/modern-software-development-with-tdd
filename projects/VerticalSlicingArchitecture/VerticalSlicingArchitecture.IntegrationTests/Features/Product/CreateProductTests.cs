using FluentAssertions;
using VerticalSlicingArchitecture.Features.Product;
using VerticalSlicingArchitecture.IntegrationTests;
using VerticalSlicingArchitecture.IntegrationTests.Shared;
using Xunit;

//https://vivasoftltd.com/docker-powered-dot-net-integration-tests-with-testcontainers/

namespace VerticalSlicingArchitecture.IntegrationTests.Features.Product
{
    public class CreateProductTests : IntegrationTestBase
    {
        public CreateProductTests(IntegrationTestWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task CreateProductShouldCreateProductInDatabase()
        {
            // Arrange
            var command = new CreateProduct.Command
            {
                Name = "AMD Ryzen 7 7700X",
                Description = "CPU",
                Price = 223.99m
            };

            // Act
            var productResponse = await PostAsync<CreateProduct.Command, CreateProduct.Response>("/api/products", command);

            var createProduct = await GetAsync<GetProduct.Response>($"/api/products/{productResponse.Id}");

            createProduct.Should().NotBeNull();
            createProduct.Name.Should().Be(command.Name);
            createProduct.Description.Should().Be("CPU");
            createProduct.Price.Should().Be(223.99m);
        }
    }
}
