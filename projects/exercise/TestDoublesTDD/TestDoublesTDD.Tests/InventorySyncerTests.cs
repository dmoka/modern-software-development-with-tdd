using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;

namespace TestDoublesTDD.Tests
{

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

    public  class InventorySyncerTests
    {
        [Test]
        public void SyncShouldDoNothingWithNoProducts()
        {
            var productRepoFake = new ProductRepositoryFake(new List<Product>());
            var externalServiceStub = new ExternalWarehouseServiceStub(new Dictionary<Guid, int>());
            var syncer = new InventorySyncer(productRepoFake, externalServiceStub, new DummyLogger());

            syncer.Sync();
        }


        [Test]
        public void SyncShouldUpdateSingleProductStockLevel()
        {
            //Arrange
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var productRepoFake = new ProductRepositoryFake(new List<Product>() { product });

            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(new Dictionary<Guid, int>()
            {
                {product.Id, 30}
            });

            //Act
            var syncer = new InventorySyncer(productRepoFake, externalWarehouseServiceStub, new DummyLogger());
            syncer.Sync();

            //Assert
            product = productRepoFake.GetProducts().First();

            product.StockLevel.Quantity.Should().Be(30);
        }

        [Test]
        public void SyncShouldLogError_whenNoStockLevelData()
        {
            //Arrange
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var productRepoFake = new ProductRepositoryFake(new List<Product>() { product });

            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(new Dictionary<Guid, int>());
            var logger = new Mock<ILogger>();

            //Act
            var syncer = new InventorySyncer(productRepoFake, externalWarehouseServiceStub, logger.Object);
            syncer.Sync();

            //Assert
            logger.Verify(mock => mock.LogWarning(It.Is<string>(l => l == $"No stock level data for product with id {product.Id}")), Times.Once);
        }

        [Test]
        public void SyncShouldUpdateMultipleProductStockLevels()
        {
            //Arrange
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Logictech WebCam v2", "The new generational webcam in 2025", 30.9m);
            var productRepoFake = new ProductRepositoryFake(new List<Product>() { product, product2 });

            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(new Dictionary<Guid, int>()
            {
                {product.Id, 30},
                {product2.Id, 40}
            });

            //Act
            var syncer = new InventorySyncer(productRepoFake, externalWarehouseServiceStub, new DummyLogger());
            syncer.Sync();

            //Assert
            productRepoFake.GetProducts().Should().SatisfyRespectively(
                p1 => p1.StockLevel.Quantity.Should().Be(30),
                p2 => p2.StockLevel.Quantity.Should().Be(40));
        }

        [Test]
        public void SyncShouldMarkProductsOutOfStockWhenQuantityBecomesZero()
        {
            //Arrange
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var productRepoFake = new ProductRepositoryFake(new List<Product>() { product });

            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(new Dictionary<Guid, int>()
            {
                {product.Id, 0}
            });

            //Act
            var syncer = new InventorySyncer(productRepoFake, externalWarehouseServiceStub, new DummyLogger());
            syncer.Sync();

            //Assert
            product = productRepoFake.GetProducts().First();

            product.StockLevel.Quantity.Should().Be(0);
            product.StockLevel.QualityStatus.Should().Be(QualityStatus.OutOfStock);
        }

        [Test]
        public void SyncShouldLogNoUpdates_whenNoProductQuantityChange()
        {
            //Arrange
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Logictech WebCam v2", "The new generational webcam in 2025", 30.9m);
            var productRepoFake = new ProductRepositoryFake(new List<Product>() { product, product2 });
            var logger = new Mock<ILogger>();
            var externalWarehouseServiceStub = new ExternalWarehouseServiceStub(new Dictionary<Guid, int>()
            {
                {product.Id, 20},
                {product2.Id, 20}
            });

            //Act
            var syncer = new InventorySyncer(productRepoFake, externalWarehouseServiceStub, logger.Object);
            syncer.Sync();

            //Assert
            productRepoFake.GetProducts().Should().SatisfyRespectively(
                p1 => p1.StockLevel.Quantity.Should().Be(20),
                p2 => p2.StockLevel.Quantity.Should().Be(20));

            logger.Verify(mock => mock.LogInfo(It.Is<string>(l => l == "No products updates")), Times.Once);
        }
    }

    public class ExternalWarehouseServiceStub : IExternalWarehouseService
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

    public class ProductRepositoryFake : IProductRepository
    {
        private readonly IEnumerable<Product> _products;

        public ProductRepositoryFake(IEnumerable<Product> products)
        {
            _products = products;
        }
        public IEnumerable<Product> GetProducts()
        {
            return _products;
        }
    }
}
