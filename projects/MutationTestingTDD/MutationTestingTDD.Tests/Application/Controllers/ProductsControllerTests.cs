using System.Net;
using FluentAssertions;
using Moq;
using MutationTestingTDD.Application.Controllers;
using MutationTestingTDD.Domain;
using MutationTestingTDD.Tests.Asserters;
using MutationTestingTDD.Tests.MutationTestingTDD.Tests;
using MutationTestingTDD.Tests.MutationTestingTDD.Tests.MutationTestingTDD.Tests;

namespace MutationTestingTDD.Tests.Application.Controllers
{
    public class ProductsControllerTests
    {
        [Test]
        public async Task GetProductShouldReturnNotFound_whenNoProductExist()
        {
            using var scope = new InMemoryTestServerScope();

            var response = await scope.Client.GetAsync($"/products/{Guid.NewGuid()}");

            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetProductShouldReturnProduct_whenProductExist()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var product = new Product("Logitech HD Pro Webcam", ProductCategory.Electronic, 200, SaleState.NoSale);
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client.GetAsync($"/products/{product.Id}");

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseMessageAsserter.AssertThat(response).HasJsonInBody(new
            {
                id = product.Id,
                name = "Logitech HD Pro Webcam",
                category = ProductCategory.Electronic,
                price = 200,
                saleState = SaleState.NoSale,
                lastPickState = (int)PickState.New,
                domainEvents = Array.Empty<object>()
            });
        }

        [Test]
        public async Task GetAllProductShouldReturnBadRequest_whenNoCategorySpecified()
        {
            using var scope = new InMemoryTestServerScope();

            var response = await scope.Client.GetAsync("/products");

            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseMessageAsserter.AssertThat(response)
                .HasTextInBody("The product category must be specified");
        }

        [Test]
        public async Task GetAllShouldReturnNoProduct_whenNothingFoundInCategory()
        {
            using var scope = new InMemoryTestServerScope();

            var response = await scope.Client.GetAsync("/products?category=Electronic");

            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseMessageAsserter.AssertThat(response).HasEmptyJsonArrayInBody();
        }

        [Test]
        public async Task GetAllShouldReturnSingleProduct_whenSingleFoundWithFilters()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var product = new Product("Logitech HD Pro Webcam", ProductCategory.Electronic, 300, SaleState.OnSale);
            await scope.AddProductsToDbContext(product);

            var product2 = new Product("Acer Webcam", ProductCategory.Electronic, 600, SaleState.OnSale);
            await scope.AddProductsToDbContext(product2);

            //Act
            var response = await scope.Client.GetAsync("/products?category=Electronic&maxPrice=400&isOnSale=true");

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseMessageAsserter.AssertThat(response).HasJsonArrayInBody(new[]
            {
                new
                {
                    id = product.Id,
                    name = "Logitech HD Pro Webcam",
                    category = ProductCategory.Electronic,
                    price = 300,
                    saleState = SaleState.OnSale,
                    lastPickState = (int)PickState.New,
                    domainEvents = Array.Empty<object>()
                }
            });
        }

        [Test]
        public async Task GetAllShouldReturnMultipleProduct_whenMultipleFoundWithFilters()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var product = new Product("Logitech HD Pro Webcam", ProductCategory.Electronic, 300, SaleState.OnSale);
            await scope.AddProductsToDbContext(product);

            var product2 = new Product("Acer Webcam", ProductCategory.Electronic, 400, SaleState.OnSale);
            await scope.AddProductsToDbContext(product2);

            var product3 = new Product("Acer Webcam", ProductCategory.Electronic, 350, SaleState.NoSale);
            await scope.AddProductsToDbContext(product3);

            //Act
            var response = await scope.Client.GetAsync("/products?category=Electronic&maxPrice=400&isOnSale=true");

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseMessageAsserter.AssertThat(response).HasJsonArrayInBody(new[]
            {
                new
                {
                    id = product2.Id,
                    name = "Acer Webcam",
                    category = ProductCategory.Electronic,
                    price = 400,
                    saleState = SaleState.OnSale,
                    lastPickState = (int)PickState.New,
                    domainEvents = Array.Empty<object>()
                },

                new
                {
                    id = product.Id,
                    name = "Logitech HD Pro Webcam",
                    category = ProductCategory.Electronic,
                    price = 300,
                    saleState = SaleState.OnSale,
                    lastPickState = (int)PickState.New,
                    domainEvents = Array.Empty<object>()
                }
            });
        }

        [Test]
        public async Task ProductShouldBeCreated_whenNoProductExistsWithName()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var newProduct = new
            {
                name = "DEWALT Screwdriver Bit Set",
                category = ProductCategory.Tool,
                price = 700,
                isOnSale = false
            };

            //Act
            var response = await scope.Client.PostAsync("/products", JsonPayloadBuilder.Build(newProduct));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Created);

            scope.WarehouseDbContext.Products.Should().SatisfyRespectively(
                p =>
                {
                    p.Name.Should().Be("DEWALT Screwdriver Bit Set");
                    p.Category.Should().Be(ProductCategory.Tool);
                    p.Price.Should().Be(700);
                    p.SaleState.Should().Be(SaleState.NoSale);
                });

            var product = scope.WarehouseDbContext.Products.Single();
            var stockLevel = scope.WarehouseDbContext.StockLevels.Single();
            stockLevel.Should().NotBeNull();
            stockLevel.ProductId.Should().Be(product.Id);
            stockLevel.Count.Should().Be(10);
        }

        [Test]
        public async Task OnSaleProductShouldBeCreated()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var newProduct = new
            {
                name = "DEWALT Screwdriver Bit Set",
                category = ProductCategory.Tool,
                price = 700,
                isOnSale = true
            };

            //Act
            var response = await scope.Client.PostAsync("/products", JsonPayloadBuilder.Build(newProduct));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Created);

            scope.WarehouseDbContext.Products.Should().SatisfyRespectively(
                p =>
                {
                    p.Name.Should().Be("DEWALT Screwdriver Bit Set");
                    p.Category.Should().Be(ProductCategory.Tool);
                    p.Price.Should().Be(700);
                    p.SaleState.Should().Be(SaleState.OnSale);
                });

            var product = scope.WarehouseDbContext.Products.Single();
            var stockLevel = scope.WarehouseDbContext.StockLevels.Single();
            stockLevel.Should().NotBeNull();
            stockLevel.ProductId.Should().Be(product.Id);
            stockLevel.Count.Should().Be(10);
        }

        [Test]
        public async Task ShouldReturnConflict_whenProductWithNameAlreadyExists()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var productName = "Logitech HD Pro Webcam";

            var product = new Product(productName, ProductCategory.Electronic, 200, SaleState.NoSale);
            await scope.AddProductsToDbContext(product);

            var newProduct = new
            {
                name = productName,
                category = ProductCategory.Tool,
                price = 700,
                isOnSasle = false
            };

            //Act
            var response = await scope.Client.PostAsync("/products", JsonPayloadBuilder.Build(newProduct));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.Conflict);
        }

        [TestCase(2,8)]
        [TestCase(10,0)]
        public async Task PickProductShouldDecreaseStockLevel(int pickCount, int expectedStockLevel)
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var product = new Product("Logitech HD Pro Webcam", ProductCategory.Electronic, 200, SaleState.NoSale);
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client.PostAsync($"/products/{product.Id}/pick", JsonPayloadBuilder.Build(new PickPayload {Count = pickCount }));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.NoContent);
            var stockLevel = scope.WarehouseDbContext.StockLevels.Single();
            stockLevel.Count.Should().Be(expectedStockLevel);
        }

        [Test]
        public async Task PickProductShouldReturnError_whenPickedCountIsBiggerThanStockLevel()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var product = new Product("Logitech HD Pro Webcam", ProductCategory.Electronic, 200, SaleState.NoSale);
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client.PostAsync($"/products/{product.Id}/pick", JsonPayloadBuilder.Build(new PickPayload { Count = 11 }));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseMessageAsserter.AssertThat(response)
                .HasTextInBody("Cannot be picked more than stock level");
        }

        [Test]
        public async Task UnpickShouldIncreaseStockLevel()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var product = new Product("Logitech HD Pro Webcam", ProductCategory.Electronic, 200, SaleState.NoSale);
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client.PostAsync($"/products/{product.Id}/unpick", JsonPayloadBuilder.Build(new PickPayload { Count = 40}));

            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.NoContent);

            product = scope.WarehouseDbContext.Products.Single();
            product.LastPickState.Should().Be(PickState.Unpicked);

            var stockLevel = scope.WarehouseDbContext.StockLevels.Single();
            stockLevel.Count.Should().Be(50);

        }

        [Test]
        public async Task UnpickShouldReturnBadReques_whenNewLevelBiggerThanMaxStockLevel()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var product = new Product("Logitech HD Pro Webcam", ProductCategory.Electronic, 200, SaleState.NoSale);
            await scope.AddProductsToDbContext(product);

            //Act
            var response = await scope.Client.PostAsync($"/products/{product.Id}/unpick", JsonPayloadBuilder.Build(new PickPayload { Count = 41 }));

            await HttpResponseMessageAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task ProductShouldBeCreated2()
        {
            //Arrange
            using var scope = new InMemoryTestServerScope();

            var newProduct = new
            {
                name = "DEWALT Screwdriver Bit Set",
                category = ProductCategory.Tool,
                price = 30
            };

            //Act
            var response = await scope.Client
                .PostAsync("/products", JsonPayloadBuilder.Build(newProduct));

            //Assert
            await HttpResponseMessageAsserter.AssertThat(response)
                .HasStatusCode(HttpStatusCode.Created);

            var product = scope.WarehouseDbContext.Products.Single();
            product.Name.Should().Be("DEWALT Screwdriver Bit Set");
            product.Category.Should().Be(ProductCategory.Tool);
            product.Price.Should().Be(30);
        }


        [Test]
        public async Task ProductShouldBeCreated()
        {
            //Arrange
            var mockedProductService = new Mock<IProductService>();
            using var scope = new InMemoryTestServerScope(mockedProductService);

            var newProduct = new
            {
                name = "DEWALT Screwdriver Bit Set",
                category = ProductCategory.Tool,
                price = 30
            };

            //Act
            await scope.Client
                .PostAsync("/products", JsonPayloadBuilder.Build(newProduct));

            //Assert
            mockedProductService.Verify(
                service => service.CreateAsync(It.Is<Product>(p =>
                    p.Name == "DEWALT Screwdriver Bit Set" &&
                    p.Category == ProductCategory.Tool &&
                    p.Price == 30
                )),
                Times.Once
            );
        }

    }

    public interface IProductService
    {
        void CreateAsync(Product p);
    }
}