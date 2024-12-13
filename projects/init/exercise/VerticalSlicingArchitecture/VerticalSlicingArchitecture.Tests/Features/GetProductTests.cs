using System.Net;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Tests.Asserters;
using VerticalSlicingArchitecture.Tests.Shared;

namespace VerticalSlicingArchitecture.Tests.Features
{
    public class GetProductTests
    {
        [Test]
        public async Task GetProductShouldReturnProduct()
        {
            using var server = new InMemoryTestServer();
            var product = new Product("Logitech Webcam", "A high-quality webcam from Logitech", 99.99m);
            server.DbContext().Products.Add(product);

            var stockLevel = new StockLevel(product.Id, 40);
            server.DbContext().StockLevels.Add(stockLevel);

            await server.DbContext().SaveChangesAsync();

            var response = await server.Client().GetAsync($"/api/products/{product.Id}");

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                name = "Logitech Webcam",
                description = "A high-quality webcam from Logitech",
                price = 99.99m,
                stockLevel = 40
            });
        }

        [Test]
        public async Task GetProductShouldReturnNotFound_whenNoProductExists()
        {
            using var server = new InMemoryTestServer();

            var response = await server.Client().GetAsync($"/api/products/{Guid.NewGuid()}");

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.NotFound);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "GetProduct.NotFound",
                description = "Product not found"
            });
        }
    }
}
