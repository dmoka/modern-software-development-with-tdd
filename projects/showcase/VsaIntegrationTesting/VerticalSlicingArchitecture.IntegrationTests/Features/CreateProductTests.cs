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

namespace VerticalSlicingArchitecture.IntegrationTests.Features
{
    public class CreateProductTests : IntegrationTestBase
    {
        private readonly WarehousingDbContext _dbContext;
        public CreateProductTests(IntegrationTestWebFactory factory) : base(factory)
        {
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<WarehousingDbContext>();
        }

        [Fact]
        public async Task CreateProductShouldPersistProductInDb()
        {
            //Arrange
            var command = new CreateProduct.Command()
            {
                Name = "Logitech webcam",
                Description = "A high quality webcam",
                Price = 100
            };

            //Act
            var productResponse = await
                PostAsync<CreateProduct.Command, CreateProduct.Response>("api/products", command);

            //Assert
            _dbContext.Products.Count().Should().Be(1);
            var product = _dbContext.Products.First();
            product.Name.Should().Be("Logitech webcam");
            product.Description.Should().Be("A high quality webcam");
            product.Price.Should().Be(100);
        }
    }
}
