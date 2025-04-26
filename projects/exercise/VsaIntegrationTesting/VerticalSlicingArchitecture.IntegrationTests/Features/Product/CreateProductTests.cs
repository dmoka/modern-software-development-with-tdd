using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Features.Product;
using VerticalSlicingArchitecture.IntegrationTests.Shared;
using Xunit;

namespace VerticalSlicingArchitecture.IntegrationTests.Features.Product
{
    public class CreateProductTests : IntegrationTestBase
    {
        private readonly WarehousingDbContext _dbContext;
        public CreateProductTests(IntegrationTestWebFactory factory) : base(factory)
        {
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<WarehousingDbContext>();
        }

        [Fact]
        public async Task CreateProductShouldStoreProductInDb()
        {
            // Arrange
            var command = new CreateProduct.Request
            {
                Name = "AMD Ryzen 7 7700X",
                Description = "CPU",
                Price = 223.99m
            };

            //Act
            await PostAsync<CreateProduct.Request, Guid>("api/products", command);

            var product = _dbContext.Products.FirstOrDefault();

            product.Should().NotBeNull();
            product.Name.Should().Be(command.Name);
            product.Description.Should().Be(command.Description);
            product.Price.Should().Be(223.99m);
        }
    }
}
