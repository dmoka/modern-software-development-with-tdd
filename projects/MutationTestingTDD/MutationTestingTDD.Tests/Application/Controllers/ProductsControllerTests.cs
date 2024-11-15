using System.Net;
using FluentAssertions;
using Moq;
using MutationTestingTDD.Application.Controllers;
using MutationTestingTDD.Domain;
using MutationTestingTDD.Tests.Asserters;

namespace MutationTestingTDD.Tests.Application.Controllers
{
    public class ProductsControllerTests
    {
        [Test]
        public async Task GetProductShouldReturnNotFound_whenNoProductExist()
        {
            using var scope = new InMemoryTestServer();

            var response = await scope.Client().GetAsync($"/products/{Guid.NewGuid()}");

            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetProductShouldReturnProduct_whenProductExist()
        {
            //Arrange
            using var scope = new InMemoryTestServer();

            var product = new Product("Logitech HD Pro Webcam", "The best webcam in 2024", 200);
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client().GetAsync($"/products/{product.Id}");

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseMessageAsserter.AssertThat(response).HasJsonInBody(new
            {
                id = product.Id,
                name = "Logitech HD Pro Webcam",
                description = "The best webcam in 2024",
                price = 200
            });
        }

        [Test]
        public async Task GetAllProductShouldReturnBadRequest_whenNoCategorySpecified()
        {
            using var scope = new InMemoryTestServer();

            var response = await scope.Client().GetAsync("/products");

            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetAllShouldReturnNoProduct_whenNothingFoundInCategory()
        {
            using var scope = new InMemoryTestServer();

            var response = await scope.Client().GetAsync("/products?searchText=asd");

            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseMessageAsserter.AssertThat(response).HasEmptyJsonArrayInBody();
        }

        [Test]
        public async Task GetAllShouldReturnSingleProduct_whenSingleFoundWithFilters()
        {
            //Arrange
            using var scope = new InMemoryTestServer();

            var product = new Product("Logitech HD Pro Webcam", "The best webcam in 2024", 200);
            await scope.AddProductsToDbContext(product);

            var product2 = new Product("Razor AMD Processor", "The best processor in 2024", 500);
            await scope.AddProductsToDbContext(product2);

            //Act
            var response = await scope.Client().GetAsync("/products?searchText=best&minPrice=0&maxPrice=400");

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseMessageAsserter.AssertThat(response).HasJsonArrayInBody(new[]
            {
                new
                {
                    id = product.Id,
                    name = "Logitech HD Pro Webcam",
                    description = "The best webcam in 2024",
                    price = 200,
                }
            });
        }

        [Test]
        public async Task ProductShouldBeCreated_whenNoProductExistsWithName()
        {
            //Arrange
            using var scope = new InMemoryTestServer();

            var newProduct = new
            {
                name = "Logitech HD Pro Webcam",
                description = "The best webcam in 2024",
                price = 700,
            };

            //Act
            var response = await scope.Client().PostAsync("/products", JsonPayloadBuilder.Build(newProduct));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Created);

            scope.DbContext().Products.Should().SatisfyRespectively(
                p =>
                {
                    p.Name.Should().Be("Logitech HD Pro Webcam");
                    p.Description.Should().Be("The best webcam in 2024");
                    p.Price.Should().Be(700);
                });

            var product = scope.DbContext().Products.Single();
            product.Should().NotBeNull();
            product.StockLevel.Should().NotBeNull();
            product.StockLevel.ProductId.Should().Be(product.Id);
            product.StockLevel.Quantity.Should().Be(20);
        }

        [Test]
        public async Task ShouldReturnConflict_whenProductWithNameAlreadyExists()
        {
            //Arrange
            using var scope = new InMemoryTestServer();

            var productName = "Logitech HD Pro Webcam";

            var product = new Product(productName, "The best logitech webcam in 2024", 200);
            await scope.AddProductsToDbContext(product);

            var newProduct = new
            {
                name = productName,
                description = "The best logitech webcam in 2025",
                price = 400,
            };

            //Act
            var response = await scope.Client().PostAsync("/products", JsonPayloadBuilder.Build(newProduct));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task PickShouldNotWork_whenProductIsExpired()
        {
            //Arrange
            using var scope = new InMemoryTestServer();

            var product = new Product("Logitech HD Pro Webcam", "The best logitech webcam in 2024", 200);
            product.StockLevel.QualityStatus = QualityStatus.Expired;
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client().PostAsync($"/products/{product.Id}/pick", JsonPayloadBuilder.Build(new PickPayload { Count = 2 }));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        }

        //False positive
        [Test]
        public async Task PickShouldNotWork_whenPickedBiggerThanMaxPickCountOperation()
        {
            //Arrange
            using var scope = new InMemoryTestServer();

            var product = new Product("Logitech HD Pro Webcam", "The best logitech webcam in 2024", 200);
            product.StockLevel.Quantity = 50;
            product.StockLevel.QualityStatus = QualityStatus.Expired;
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client().PostAsync($"/products/{product.Id}/pick", JsonPayloadBuilder.Build(new PickPayload { Count = 35 }));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        }


        [Test]
        public async Task PickProductShouldDecreaseStockLevel()
        {
            //Arrange
            using var scope = new InMemoryTestServer();

            var product = new Product("Logitech HD Pro Webcam", "The best logitech webcam in 2024", 200);
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client().PostAsync($"/products/{product.Id}/pick", JsonPayloadBuilder.Build(new PickPayload {Count = 2 }));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.NoContent);
            product = scope.DbContext().Products.Single();
            product.StockLevel.Quantity.Should().Be(18);
        }

        [Test]
        public async Task PickProductShouldReturnError_whenPickedCountIsBiggerThanStockLevel()
        {
            //Arrange
            using var scope = new InMemoryTestServer();

            var product = new Product("Logitech HD Pro Webcam", "The best logitech webcam in 2024", 200);
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client().PostAsync($"/products/{product.Id}/pick", JsonPayloadBuilder.Build(new PickPayload { Count = 21 }));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseMessageAsserter.AssertThat(response)
                .HasTextInBody("Insufficient stock quantity to pick");
        }
    }
}