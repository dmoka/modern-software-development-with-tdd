using FluentAssertions;

namespace DomainTDD.Tests
{
    public class ProductTests
    {
        [Test]
        public void CreateProductShouldWork_WhenRequiredDataIsSpecified()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);

            product.Id.Should().NotBeEmpty();
            product.LastPickStatus.Should().Be(PickStatus.None);
            product.StockLevel.Should().NotBeNull();
        }

        [Test]
        public void CreateProductShouldInitStockLevel()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);
            
            product.StockLevel.Should().NotBeNull();
            product.StockLevel.Quantity.Should().Be(10);
            product.StockLevel.QualityStatus.Should().Be(QualityStatus.Available);
        }
    }
}