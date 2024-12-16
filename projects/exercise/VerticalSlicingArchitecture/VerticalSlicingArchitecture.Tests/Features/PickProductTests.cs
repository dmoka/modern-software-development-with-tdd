using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Tests.Asserters;
using VerticalSlicingArchitecture.Tests.Shared;

namespace VerticalSlicingArchitecture.Tests.Features
{
    public class PickProductTests
    {
        [Test]
        public async Task PickProductShouldDecreaseStock()
        {
            using var server = new InMemoryTestServer();
            var product = new Product("Logitech Webcam", "A high-quality webcam from Logitech", 99.99m);
            var stockLevel = new StockLevel(product.Id, 40);
            server.DbContext().Products.Add(product);
            server.DbContext().StockLevels.Add(stockLevel);
            await server.DbContext().SaveChangesAsync();

            var pickPayload = new
            {
                count = 10
            };
            var response = await server.Client().PostAsync($"/api/products/{product.Id}/pick", JsonPayloadBuilder.Build(pickPayload));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            product = server.DbContext().Products.AsNoTracking().Include(p => p.StockLevel).SingleOrDefault();

            product.StockLevel.Quantity.Should().Be(30);
        }

        [Test]
        public async Task PickProductShouldReturnBadRequest_whenNoProductIdSpecified()
        {
            using var server = new InMemoryTestServer();
            var pickPayload = new
            {
                count = 10
            };

            var response = await server.Client().PostAsync($"/api/products/{Guid.Empty}/pick", JsonPayloadBuilder.Build(pickPayload));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "PickProduct.Validation",
                description = "The product is must be filled"
            });
        }

        [Test]
        public async Task PickProductShouldReturnBadRequest_whenPickCountIsZero()
        {
            using var server = new InMemoryTestServer();
            var pickPayload = new
            {
                count = 0
            };

            var response = await server.Client().PostAsync($"/api/products/{Guid.NewGuid()}/pick", JsonPayloadBuilder.Build(pickPayload));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "PickProduct.Validation",
                description = "The pick count must be bigger than 0"
            });
        }

        [Test]
        public async Task ShouldLeadToConflict_whenPickingMoreThanAvailable()
        {
            using var server = new InMemoryTestServer();
            var product = new Product("Logitech Webcam", "A high-quality webcam from Logitech", 99.99m);
            var stockLevel = new StockLevel(product.Id, 40);
            server.DbContext().Products.Add(product);
            server.DbContext().StockLevels.Add(stockLevel);
            await server.DbContext().SaveChangesAsync();

            var pickPayload = new
            {
                count = 41
            };
            var response = await server.Client().PostAsync($"/api/products/{product.Id}/pick", JsonPayloadBuilder.Build(pickPayload));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Conflict);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "PickProduct.InsufficientStock",
                description = "Insufficient stock"
            });
        }

        [Test]
        public async Task ShouldLeadToConflict_whenNoProductFound()
        {
            using var server = new InMemoryTestServer();

            var pickPayload = new
            {
                count = 10
            };
            var response = await server.Client().PostAsync($"/api/products/{Guid.NewGuid()}/pick", JsonPayloadBuilder.Build(pickPayload));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Conflict);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "PickProduct.NotFound",
                description = "Product not found"
            });
        }
    }
}
