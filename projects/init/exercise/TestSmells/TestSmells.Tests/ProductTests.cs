using FluentAssertions;
using System.Buffers;

namespace TestSmells.Tests
{
    public class ProductTests
    {
        private Product SampleProduct() => new Product("VisionOne 27", "Ultra wide monitor with HDR", 200);

        [Test]
        public void Test1()
        {
            var product = new Product("product", "desc", 0);
            product.Pick(2);
            Assert.That(product.StockLevel.Quantity, Is.EqualTo(18));
        }


        [Test]
        public void PickAndUnpickShouldChangeLastOperation()
        {
            var product = new Product("AMD Razor Processor", "The best processor in 2024", 150);
            Assert.That(product.LastOperation, Is.EqualTo(PickStatus.None));

            product.Pick(3);
            Assert.That(product.LastOperation, Is.EqualTo(PickStatus.Picked));

            product.Unpick(2);
            Assert.That(product.LastOperation, Is.EqualTo(PickStatus.Unpicked));
        }

        [Test]
        public void TestSalesName()
        {
            var product = SampleProduct();

            var name = product.GetSalesName();

            Assert.That(name, Is.EqualTo("VisionOne 27 - Ultra wide monitor with HDR"));
        }

        [Test]
        public void PickWithLoopShouldUpdateStock()
        {
            var product = new Product("Headphones", "Noise-canceling headphones", 250);
            for (int i = 0; i < 5; i++)
            {
                product.Pick(1);
            }
            Assert.That(product.StockLevel.Quantity, Is.EqualTo(15));
        }

        [Test]
        public void UnpickShouldIncreaseStock()
        {
            // Arrange
            var product = new Product("Mousepad", "Gaming mousepad", 20); // Create product object

            // Act
            product.Unpick(5); // Unpick product

            // Assert
            Console.WriteLine(product.StockLevel.Quantity); // Verify stock level
        }

  

        [Test]
        public void PickProductShouldFail_whenProductIsDamaged()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            product.StockLevel.QualityStatus = QualityStatus.Damaged;
            var pickProductAction = () => product.Pick(5);
            pickProductAction.Should().Throw<ApplicationException>().WithMessage("Can't pick damaged product");
        }

        [Test]
        public void PickShouldFail_whenPickQuanityExceedsMaxPickCount()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);

            try
            {
                product.Pick(11);
            }
            catch (ApplicationException e)
            {
                var exceptedException = new ApplicationException("Pick Quantity exceeds max pick count");
                Assert.True(e.GetType() == exceptedException.GetType());
                Assert.True(e.Message == exceptedException.Message);
            }
        }

    }

}