using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RefactoringLegacyCode.Entities;
using RefactoringLegacyCode.Features.Product;
using RefactoringLegacyCode.Tests.Asserters;
using RefactoringLegacyCode.Tests.Shared;

namespace RefactoringLegacyCode.Tests.Features.Product
{
    public class UnpickProductTests
    {
        [Test]
        public async Task UnpickProductShouldReduceStock()
        {
            using var testServer = new InMemoryTestServer();
            var product = await CreateProduct(testServer, 10);

            var command = new UnpickProduct.Endpoint.Command(product.Id, 3);
            var response = await testServer.Client()
                .PostAsync($"api/products/{product.Id}/unpick", JsonPayloadBuilder.Build(command));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            product = testServer.DbContext().Products.AsNoTracking().Include(p => p.StockLevel).SingleOrDefault();
            product.StockLevel.Quantity.Should().Be(13);
            product.LastOperation.Should().Be(LastOperation.Unpicked);
        }

        [Test]
        public async Task UnpickShouldFail_WhenProductDoesNotExist()
        {
            using var testServer = new InMemoryTestServer();

            var productId = Guid.NewGuid();
            var command = new UnpickProduct.Endpoint.Command(productId, 3);
            var response = await testServer.Client()
                .PostAsync($"api/products/{productId}/unpick", JsonPayloadBuilder.Build(command));

           await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
           await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
           {
               code = "UnpickProduct.Validation",
               description = $"Product with id {productId} doesn't exist"
           });
        }

        [Test]
        public async Task UnpickShouldFail_WhenNoProductIdSpecified()
        {
            using var testServer = new InMemoryTestServer();

            var productId = Guid.Empty;
            var command = new UnpickProduct.Endpoint.Command(productId, 3);
            var response = await testServer.Client()
                .PostAsync($"api/products/{productId}/unpick", JsonPayloadBuilder.Build(command));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "UnpickProduct.Validation",
                description = "'Product Id' must not be empty."
            });
        }

        [Test]
        public async Task UnpickShouldFail_WhenInvalidQuantitySpecified()
        {
            using var testServer = new InMemoryTestServer();

            var productId = Guid.NewGuid();
            var command = new UnpickProduct.Endpoint.Command(productId, 0);
            var response = await testServer.Client()
                .PostAsync($"api/products/{productId}/unpick", JsonPayloadBuilder.Build(command));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "UnpickProduct.Validation",
                description = "UnpickCount must be greater than 0"
            });
        }

        [Test]
        public async Task UnpickShouldFail_WhenStockReachesMoreThanMaxLimit()
        {
            using var testServer = new InMemoryTestServer();
            var product = await CreateProduct(testServer, 45);

            var command = new UnpickProduct.Endpoint.Command(product.Id, 6);
            var response = await testServer.Client()
                .PostAsync($"api/products/{product.Id}/unpick", JsonPayloadBuilder.Build(command));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "UnpickProduct.Validation",
                description = "Cannot unpick more than max stock level"
            });
        }


        private static async Task<Entities.Product> CreateProduct(InMemoryTestServer testServer, int quantity)
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

}
