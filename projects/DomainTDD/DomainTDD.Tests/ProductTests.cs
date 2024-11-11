using FluentAssertions;

namespace DomainTDD.Tests
{
    public class ProductTests
    {
        [Test]
        public void CreateProductInitFields()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);

            product.Id.Should().NotBeEmpty();
            product.LastOperation.Should().Be(PickStatus.None);
            product.StockLevel.Should().NotBeNull();
        }

        [Test]
        public void CreateProductShouldInitStockLevel()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);
            
            product.StockLevel.Should().NotBeNull();
            product.StockLevel.Quantity.Should().Be(20);
            product.StockLevel.QualityStatus.Should().Be(QualityStatus.Available);
            product.StockLevel.Id.Should().NotBeEmpty();
        }

        [Test]
        public void PickProductShouldDecreaseQuantity()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);

            product.Pick(5);

            product.StockLevel.Quantity.Should().Be(15);
            product.LastOperation.Should().Be(PickStatus.Picked);
        }

        [Test]
        public void PickProductShouldThrowError_whenPickedQuanitiyBiggerThanAvailable()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);

            product.Pick(10);
            product.Pick(10);

            var pickProductAction = () => product.Pick(1);

            pickProductAction.Should().Throw<ApplicationException>().WithMessage("Insufficient stock quantity to pick");
        }

        [Test]
        public void PickProductShouldFail_whenProductIsDamaged()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);
            product.StockLevel.QualityStatus = QualityStatus.Damaged;

            var pickProductAction = () => product.Pick(5);

            pickProductAction.Should().Throw<ApplicationException>().WithMessage("Can't pick damaged product");
        }

        [Test]
        public void PickProductShouldFail_whenProductIsExpired()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);
            product.StockLevel.QualityStatus = QualityStatus.Expired;

            var pickProductAction = () => product.Pick(5);

            pickProductAction.Should().Throw<ApplicationException>().WithMessage("Can't pick damaged product");
        }


        [Test]
        public void PickShouldFail_whenPickQuanityExceedsMaxPickCount()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);

            var pickProductAction = () => product.Pick(11);

            pickProductAction.Should().Throw<ApplicationException>().WithMessage("Pick Quantity exceeds max pick count");
        }

        [Test]
        public void UnPickShouldIncreaseStock()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);

            product.Unpick(11);

            product.StockLevel.Quantity.Should().Be(42);
        }

        [Test]
        public void UnpickShouldFail_whenGoesOverMaxStockLevel()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9);

            var unpickProductAction = () => product.Unpick(31);

            unpickProductAction.Should().Throw<ApplicationException>();
        }

    }
}