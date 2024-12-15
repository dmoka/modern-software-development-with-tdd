using DomainTDD;
using FluentAssertions;

namespace TestSmells.Tests
{
    public class ProductSearcherTests
    {
        private Product Product1 = new Product("Xerox monitor", "The best monitor in 2024", 201);
        private Product Product2 = new Product("AMD Process", "The best processor", 199);
        private Product Product3 = new Product("Logitech webcam", "The best webcam in 2024", 400);

        [Test]
        public void TestSearch()
        {
            var p = new List<Product>();

            var sr= ProductSearcher.Search(p, "dummy");

            sr.Should().BeEmpty();
        }

        [Test]
        public void ExceptionCheckWithNullValue()
        {
            try
            {
                ProductSearcher.Search(null, "dummy");
            }
            catch (Exception e)
            {
                e.Should().BeOfType<ApplicationException>();
            }

        }

        [Test]
        public void ShouldFilterProduct_whenPriceIsEqualToMinPrice()
        {
            var product1 = new Product("Product1", "desc1", 199);
            var product2 = new Product("Product1", "desc2", 200);
            var product3 = new Product("Product1", "desc3", 201);
            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "", 200);

            searchResult.Should().BeEquivalentTo(new List<Product>() { product2, product3 });
        }

        [Test]
        public void TestPriceErrors()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 199);
            var products = new List<Product>() { product1 };
            var searchAction = () =>  ProductSearcher.Search(products, "", -1);
            searchAction.Should().Throw<ApplicationException>().WithMessage("The MinPrice be bigger or equal to 0");
            searchAction = () => ProductSearcher.Search(products, "", 0, -4);
            searchAction.Should().Throw<ApplicationException>().WithMessage("The MaxPrice be bigger or equal to 0");
            searchAction = () => ProductSearcher.Search(products, "", 100, 50);
            searchAction.Should().Throw<ApplicationException>().WithMessage("The MinPrice should be smaller than MaxPrice");
        }


        [Test]
        public void ItShouldCorrectlyFindExpiredProduct_WhenExpiredStatusIsSetForProduct()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 199);
            product1.StockLevel.QualityStatus = QualityStatus.Expired;
            var product2 = new Product("Logitech webcam", "The best webcam in 2024", 400);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 201);

            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", null, null, QualityStatus.Damaged);

            searchResult.Should().BeEquivalentTo(new List<Product>() { product1 });
        }

        [Test]
        public void ShouldSortProductsByNameInAscendingOrder()
        {
            var products = new List<Product>() { Product1, Product2, Product3};

            var searchResult = ProductSearcher.Search(products, "best", null, null, QualityStatus.Available, ProductSearcher.Sorting.ByNameAscending);

            searchResult.Should().Equal(new List<Product>() { Product2, Product3, Product1 });
        }

        [Test]
        public void ShouldSortProductsByNameInDescendingOrder()
        {
            var products = new List<Product>() { Product1, Product2, Product3 };

            var searchResult = ProductSearcher.Search(products, "best", null, null, QualityStatus.Available, ProductSearcher.Sorting.ByNameDescending);

            searchResult.Should().Equal(new List<Product>() { Product1, Product3, Product2 });
        }

        [Test]
        public void CheckRangeForMultiplePrices()
        {
            var prices = new List<decimal>() { 200, 250, 300, 350, 400};

            for (int i = 0; i < prices.Count; i++)
            {
                var product = new Product("Product", "desc", prices[i] + 1000);
                var product2 = new Product("Product", "desc", prices[i]);
                var products = new List<Product>() {product, product2};

                var searchResult = ProductSearcher.Search(products, "", 200, 400, QualityStatus.Available, ProductSearcher.Sorting.ByPriceAscending);

                searchResult.Should().Equal(new List<Product>() { product2 });
            }

        }

        [Test]
        public void ShouldSortProductsByPriceDescending()
        {
            var product1 = new Product("Xerox monitor", "The best monitor in 2024", 201);
            var product2 = new Product("AMD Process", "The best processor", 500);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 400);

            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", null, null, QualityStatus.Available, ProductSearcher.Sorting.ByPriceDescending);

            searchResult[0].Should().Be(product2);
            searchResult[1].Should().Be(product3);
            searchResult[2].Should().Be(product1);
        }

        [Test]
        public void ShouldSearchAndSortAndFilterByPrice()
        {
            var product1 = new Product("Webcam", "High-quality webcam", 199);
            var product2 = new Product("Mouse", "Wireless mouse", 150);
            var product3 = new Product("Keyboard", "Mechanical keyboard", 100);

            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "High-quality");
            searchResult.Should().Contain(product1);

            var filteredResult = ProductSearcher.Search(products, "", 150, 200);
            filteredResult.Should().BeEquivalentTo(new List<Product> { product1, product2 });

            var sortedResult = ProductSearcher.Search(products, "", null, null, QualityStatus.Available, ProductSearcher.Sorting.ByPriceAscending);
            sortedResult.Should().Equal(new List<Product> { product3, product2, product1});
        }
    }
}
