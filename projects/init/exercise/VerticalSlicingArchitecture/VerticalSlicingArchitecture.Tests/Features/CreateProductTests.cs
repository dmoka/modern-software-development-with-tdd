using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework.Constraints;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Features;
using VerticalSlicingArchitecture.Tests.Asserters;
using VerticalSlicingArchitecture.Tests.Shared;

namespace VerticalSlicingArchitecture.Tests.Features
{
    public class CreateProductTests
    {
        [Test]
        public async Task CreateProductShouldPersistProductInWarehouse()
        {
            using var testServer = new InMemoryTestServer();

            var productFields = new
            {
                Name = "Logitech Webcam",
                Description = "A high-quality webcam from Logitech",
                Price = 99.99m,
                InitialStockLevel = 40
            };

            var response = await testServer.Client().PostAsync("/api/products", JsonPayloadBuilder.Build(productFields));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Created);

            var product = await testServer.DbContext().Products.Include(p => p.StockLevel).SingleOrDefaultAsync();
            product.Id.Should().NotBeEmpty();
            product.Name.Should().Be("Logitech Webcam");
            product.Description.Should().Be("A high-quality webcam from Logitech");
            product.Price.Should().Be(99.99m);
            product.StockLevel.Quantity.Should().Be(40);
        }

        [Test]
        public async Task CreateProductShouldReturnBadRequestWhenWhenNameIsNotFilled()
        {
            using var testServer = new InMemoryTestServer();

            var productFields = new
            {
                Name = "",
                Description = "A high-quality webcam from Logitech",
                Price = 99.99m,
                InitialStockLevel = 40
            };

            var response = await testServer.Client().PostAsync("/api/products", JsonPayloadBuilder.Build(productFields));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "CreateProduct.Validation",
                description = "Name is required"
            });
        }

        [Test]
        public async Task CreateProductShouldReturnBadRequestWhenWhenDecIsNotFilled()
        {
            using var testServer = new InMemoryTestServer();

            var productFields = new
            {
                Name = "Logitech webcam",
                Description = "",
                Price = 99.99m,
                InitialStockLevel = 40
            };

            var response = await testServer.Client().PostAsync("/api/products", JsonPayloadBuilder.Build(productFields));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "CreateProduct.Validation",
                description = "Description is required"
            });
        }

        [Test]
        public async Task CreateProductShouldReturnBadRequestWhenPriceIsZero()
        {
            using var testServer = new InMemoryTestServer();

            var productFields = new
            {
                Name = "Logitech webcam",
                Description = "A high-quality webcam from Logitech",
                Price = 0m,
                InitialStockLevel = 40
            };

            var response = await testServer.Client().PostAsync("/api/products", JsonPayloadBuilder.Build(productFields));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "CreateProduct.Validation",
                description = "Price must be greater than zero"
            });
        }

        [Test]
        public async Task CreateProductShouldReturnConflictWhenStockExceedsMaxStockLevel()
        {
            using var testServer = new InMemoryTestServer();

            var productFields = new
            {
                Name = "Logitech webcam",
                Description = "A high-quality webcam from Logitech",
                Price = 20m,
                InitialStockLevel = 51
            };

            var response = await testServer.Client().PostAsync("/api/products", JsonPayloadBuilder.Build(productFields));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Conflict);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "StockLevel.ExceedsMaximum",
                description = "Stock exceeds maximum level"
            });
        }

        [Test]
        public async Task CreateProductShouldReturnConflictWhenStockDoesntReachMinimum()
        {
            using var testServer = new InMemoryTestServer();

            var productFields = new
            {
                Name = "Logitech webcam",
                Description = "A high-quality webcam from Logitech",
                Price = 20m,
                InitialStockLevel = 9
            };

            var response = await testServer.Client().PostAsync("/api/products", JsonPayloadBuilder.Build(productFields));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Conflict);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "StockLevel.BelowMinimum",
                description = "Stock doesn't reach minimum"
            });
        }

        [Test]
        public async Task CreateProductShouldLeadToConflict_whenProductWithSameNameALreadyExist()
        {
            using var testServer = new InMemoryTestServer();
            var productName = "Logitech webcam";
            var product = new Product(productName, "A high-quality webcam from Logitech", 20m);
            testServer.DbContext().Products.Add(product);
            await testServer.DbContext().SaveChangesAsync();

            var productFields = new
            {
                Name = productName,
                Description = "A high-quality webcam from Logitech",
                Price = 99.99m,
                InitialStockLevel = 40
            };

            var response = await testServer.Client().PostAsync("/api/products", JsonPayloadBuilder.Build(productFields));

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Conflict);

            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                code = "CreateProduct.AlreadyExists",
                description = "Product with name already exists."
            });
        }

    }
}
