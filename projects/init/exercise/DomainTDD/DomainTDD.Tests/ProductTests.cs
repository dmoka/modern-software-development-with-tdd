using FluentAssertions;

namespace DomainTDD.Tests
{
    public class ProductTests
    {

        [Test]
        public void Product_ShouldBeCreated()
        {
            var product = new Product("Logitech Webcam", "The best webcam in 2024", 100.3m);
            
            product.Name.Should().Be("Logitech Webcam");
            product.Description.Should().Be("The best webcam in 2024");
            product.Price.Should().Be(100.3m);

            product.LastPickOperation.Should().Be(PickOperation.None);
            product.StockLevel.Quantity.Should().Be(20);
            product.StockLevel.QualityStatus.Should().Be(QualityStatus.Available);
        }

        [Test]
        public void PickShouldReduceStockLevel()
        {
            var product = new Product("Logitech Webcam", "The best webcam in 2024", 100.3m);

            product.Pick(5);
            
            product.StockLevel.Quantity.Should().Be(15);
        }
        
        [Test]
        public void PickShouldSetLastPickStatusToPicked()
        {
            var product = new Product("Logitech Webcam", "The best webcam in 2024", 100.3m);

            product.Pick(5);
            
            product.LastPickOperation.Should().Be(PickOperation.Picked);
        }

        [Test]
        public void PickShouldResultInErrorWhenPickedMoreThanStocklevel()
        {
            var product = new Product("Logitech Webcam", "The best webcam in 2024", 100.3m);
            product.Pick(10);
            product.Pick(9);
            
            var pickAction = () => product.Pick(2);

            pickAction.Should().Throw<ApplicationException>().WithMessage("Cannot pick more than two stock levels");
        }

       [Test]
        public void PickShouldResultInErrorWhenStockStatusIsExpired()
        {
            var product = new Product("Logitech Webcam", "The best webcam in 2024", 100.3m);
            product.StockLevel.QualityStatus = QualityStatus.Expired;
            
            var pickAction = () => product.Pick(2);

            pickAction.Should().Throw<ApplicationException>().WithMessage("Cannot pick expired product");

        }
        
        [Test]
        public void PickShouldResultInErrorWhenStockStatusIsDamaged()
        {
            var product = new Product("Logitech Webcam", "The best webcam in 2024", 100.3m);
            product.StockLevel.QualityStatus = QualityStatus.Damaged;
            
            var pickAction = () => product.Pick(2);

            pickAction.Should().Throw<ApplicationException>().WithMessage("Cannot pick expired product");

        }
        
        [Test]
        public void PickShouldResultInErrorWhenReachedMaxPickLimit()
        {
            var product = new Product("Logitech Webcam", "The best webcam in 2024", 100.3m);
            
            var pickAction = () => product.Pick(11);

            pickAction.Should().Throw<ApplicationException>().WithMessage("Cannot pick more than max pick limit");
        }
    }
}
