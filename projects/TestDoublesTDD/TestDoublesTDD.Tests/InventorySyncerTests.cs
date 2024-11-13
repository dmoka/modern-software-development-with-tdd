using FluentAssertions;
using Moq;

namespace TestDoublesTDD.Tests
{
    public class InventorySyncerTests
    {
        public static Product CreateProductWithStockLevel20()
        {
            var product = new Product("Logitech webcam", "The best webcam in 2024", 100);

            return product;
        }

        public static Product CreateProductWithStockLevel15()
        {
            var product = new Product("Logitech webcam", "The best webcam in 2024", 100);
            product.Pick(5);

            return product;
        }

        public class ProductRepositoryFake : IProductRepository
        {
            private readonly IList<Product> _products;

            public ProductRepositoryFake(IList<Product> products)
            {
                _products = products;
            }

            public IList<Product> GetProducts()
            {
                return _products;
            }
        }

        public class ExternalWarehouseServiceStub : ExternalWarehouseService
        {
            private readonly IDictionary<Guid, int> _stockLevels;

            public ExternalWarehouseServiceStub(IDictionary<Guid, int> stockLevels)
            {
                _stockLevels = stockLevels;
            }

            public IDictionary<Guid, int> GetStockLevels()
            {
                return _stockLevels;
            }
        }

        public class EmptyExternalWarehouseServiceStub : ExternalWarehouseService
        {
            public IDictionary<Guid, int> GetStockLevels()
            {
                return new Dictionary<Guid, int>()
                {
                };
            }
        }

        public class DummyLogger : ILogger
        {
            public void LogWarning(string message)
            {
                throw new NotImplementedException();
            }

            public void LogInfo(string message)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void SyncerShouldDoNothingWithNoProducts()
        {
            //Arrange
            var emailSenderMock = new Mock<INotificationEmailSender>();

            var prices = new Dictionary<Guid, int>()
            {
                {CreateProductWithStockLevel20().Id, 15},
                {CreateProductWithStockLevel15().Id, 10}
            };

            var productRepoFake = new ProductRepositoryFake(new List<Product>() { });
            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(prices);

            //Act
            new InventorySyncer(productRepoFake, externalWarehouseServiceStub, new DummyLogger(), emailSenderMock.Object).Sync();
        }


        [Test]
        public void SyncerShouldUpdateSingleProduct()
        {
            //Arrange
            var emailSenderMock = new Mock<INotificationEmailSender>();

            var productWithStockLevel20 = CreateProductWithStockLevel20();
            var prices = new Dictionary<Guid, int>()
            {
                {productWithStockLevel20.Id, 15},
            };

            var productRepoFake = new ProductRepositoryFake(new List<Product>() { productWithStockLevel20 });
            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(prices);

            //Act
            new InventorySyncer(productRepoFake, externalWarehouseServiceStub, new DummyLogger(), emailSenderMock.Object).Sync();

            //Assert
            var updatedProduct = productRepoFake.GetProducts().Single();
            updatedProduct.StockLevel.Quantity.Should().Be(15);
        }

        [Test]
        public void ShouldLogError_whenNoStockLevelData()
        {
            //Arrange
            var emailSenderMock = new Mock<INotificationEmailSender>();

            var productWithStockLevel20 = CreateProductWithStockLevel20();

            var productRepoFake = new ProductRepositoryFake(new List<Product>() { productWithStockLevel20 });
            var externalWarehouseServiceStub = new EmptyExternalWarehouseServiceStub();
            var loggerMock = new Mock<ILogger>();

            //Act
            new InventorySyncer(productRepoFake, externalWarehouseServiceStub, loggerMock.Object, emailSenderMock.Object).Sync();

            //Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(msg => msg == $"No stock level data found for product {productWithStockLevel20.Id}")));
        }


        [Test]
        public void SyncerShouldUpdateMultipleProducts()
        {
            //Arrange
            var emailSenderMock = new Mock<INotificationEmailSender>();

            var productWithStockLevel20 = CreateProductWithStockLevel20();
            var productWithStockLevel15 = CreateProductWithStockLevel15();

            var prices = new Dictionary<Guid, int>()
            {
                {productWithStockLevel20.Id, 15},
                {productWithStockLevel15.Id, 10},
            };

            var productRepoFake = new ProductRepositoryFake(new List<Product>() { productWithStockLevel20, productWithStockLevel15 });
            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(prices);

            //Act
            new InventorySyncer(productRepoFake, externalWarehouseServiceStub, new DummyLogger(), emailSenderMock.Object).Sync();

            //Assert
            productRepoFake.GetProducts().Should().SatisfyRespectively(
                p1 => p1.StockLevel.Quantity.Should().Be(15),
                p2 => p2.StockLevel.Quantity.Should().Be(10));
        }

        [Test]
        public void ShouldContinue_whenNoStockLevelForFirst()
        {
            //Arrange
            var emailSenderMock = new Mock<INotificationEmailSender>();

            var productWithStockLevel20 = CreateProductWithStockLevel20();
            var productWithStockLevel15 = CreateProductWithStockLevel15();
            var prices = new Dictionary<Guid, int>()
            {
                {productWithStockLevel15.Id, 10},
            };

            var productRepoFake = new ProductRepositoryFake(new List<Product>() { productWithStockLevel20, productWithStockLevel15 });
            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(prices);

            var loggerMock = new Mock<ILogger>();

            //Act
            new InventorySyncer(productRepoFake, externalWarehouseServiceStub, loggerMock.Object, emailSenderMock.Object).Sync();

            //Assert
            var updated2ndProduct = productRepoFake.GetProducts().Where(p => p.Id == productWithStockLevel15.Id).SingleOrDefault();
            updated2ndProduct.StockLevel.Quantity.Should().Be(10);
        }

        [Test]
        public void ShouldMarkProductOutOfStock_whenStockLevelFalsToZero()
        {
            //Arrange
            var emailSenderMock = new Mock<INotificationEmailSender>();

            var productWithStockLevel20 = CreateProductWithStockLevel20();
            var productRepoFake = new ProductRepositoryFake(new List<Product>() { productWithStockLevel20 });
            
            var prices = new Dictionary<Guid, int>()
            {
                {productWithStockLevel20.Id, 0},
            };

            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(prices);

            //Act
            new InventorySyncer(productRepoFake, externalWarehouseServiceStub, new DummyLogger(),emailSenderMock.Object).Sync();

            //Assert
            var updatedProduct = productRepoFake.GetProducts().Single();

            updatedProduct.StockLevel.Quantity.Should().Be(0);
            updatedProduct.StockLevel.QualityStatus.Should().Be(QualityStatus.OutOfStock);
        }


        [Test]
        public void SyncerLogInfoWhenNothingWasUpdated()
        {
            //Arrange
            var emailSenderMock = new Mock<INotificationEmailSender>();

            var productWithStockLevel20 = CreateProductWithStockLevel20();
            var loggerMock = new Mock<ILogger>();

            var prices = new Dictionary<Guid, int>()
            {
                {productWithStockLevel20.Id, 20},
            };

            var productRepoFake = new ProductRepositoryFake(new List<Product>() { productWithStockLevel20 });
            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(prices);

            //Act
            new InventorySyncer(productRepoFake, externalWarehouseServiceStub, loggerMock.Object, emailSenderMock.Object).Sync();

            //Assert
            loggerMock.Verify(l => l.LogInfo(It.Is<string>(msg => msg == "No stock level update done")));
        }

        [Test]
        public void SyncerShouldSendAlertEmailWhenUnexpectedErroHappens()
        {
            //Arrange
            var emailSenderMock = new Mock<INotificationEmailSender>();

            var prices = new Dictionary<Guid, int>()
            {
                {CreateProductWithStockLevel20().Id, 15},
                {CreateProductWithStockLevel15().Id, 10}
            };

            var productRepoFake = new Mock<IProductRepository>();
            productRepoFake.Setup(p => p.GetProducts()).Throws(new ApplicationException("DB connection error"));
            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(prices);

            //Act
            new InventorySyncer(productRepoFake.Object, externalWarehouseServiceStub, new DummyLogger(), emailSenderMock.Object).Sync();

            //Assert
            emailSenderMock.Verify(mock => mock.Send("admin@tdd.com", "Alert: Unexpected Sync Error", "The inventory synchronization process failed unexpectedly: DB connection error."));
        }

    }

}