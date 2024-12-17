using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Features.Product;
using VerticalSlicingArchitecture.IntegrationTests.Shared;
using Xunit;

namespace VerticalSlicingArchitecture.IntegrationTests.Features
{
    public class PickProductTests :  IntegrationTestBase
    {
        private readonly WarehousingDbContext _dbContext;
        public PickProductTests(IntegrationTestWebFactory factory) : base(factory)
        {
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<WarehousingDbContext>();
        }

        [Fact]
        public async Task PickProductShouldDecreaseStockLevel()
        {
            //Arrange
            var command = new CreateProduct.Command()
            {
                Name = "Logitech webcam",
                Description = "A high quality webcam",
                Price = 100,
                InitialStock = 50
            };

            var productResponse = await
                PostAsync<CreateProduct.Command, CreateProduct.Response>("api/products", command);

            //Act
            var pickProductCommand = new PickProduct.Command(productResponse.Id, 10);

            await PostAsync($"api/products/{productResponse.Id}/pick", pickProductCommand);

            //Assert
            var stockLevel = _dbContext.StockLevels.Single();
            stockLevel.Quantity.Should().Be(40);
        }
    }
    
}
