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
    public class PickProductTests : IntegrationTestBase
    {
        private readonly WarehousingDbContext _dbContext;
        public PickProductTests(IntegrationTestWebFactory factory) : base(factory)
        {
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<WarehousingDbContext>();
        }

        [Fact]
        public async Task PickShouldDecreaseStocklevel()
        {
            var command = new CreateProduct.Request
            {
                Name = "AMD Ryzen 7 7700X",
                Description = "CPU",
                Price = 223.99m,
                InitialStock = 10
            };

            var productId = await PostAsync<CreateProduct.Request, Guid>("/api/products", command);

            //Act
            var pickProductCommand = new PickProduct.Request(productId, 3);
            await PostAsync($"/api/products/{productId}/pick", pickProductCommand);

            //Assert
            var stockLevel = _dbContext.StockLevels.SingleOrDefault(s => s.ProductId == productId);
            stockLevel.Quantity.Should().Be(7);
        }
    }
}
