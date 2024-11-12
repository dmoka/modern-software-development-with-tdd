using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainTDD;
using FluentAssertions;

//TODO: in init example, change hte price to decimal
//TODO: use the updated readme

namespace ZombiesTDD.Tests
{
    public class ProductSearcherTests
    {
        [Test]
        public void ShouldReturnNoProduct_whenNoProductPassed()
        {
            var products = new List<Product>();

            var searchResult = ProductSearcher.Search(products, "dummy");

            searchResult.Should().BeEmpty();
        }

        [Test]
        public void ShouldThrowException_whenNullPassedAsProducts()
        {
            Action act = () => ProductSearcher.Search(null, "dummy");

            act.Should().Throw<ApplicationException>().WithMessage("The provided list cannot be null");
        }

        [Test]
        public void ShouldReturnOneProduct_whenMatchedWithSingle()
        {
            var product = new Product("Logitech webcam", "The best webcam in 2024", 200);

            var products = new List<Product>() { product };

            var searchResult = ProductSearcher.Search(products, "webcam");

            searchResult.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnOneProduct_whenMatchedWithSingleFromMany()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 200);
            var product2 = new Product("AMD Razor headset", "The best headset in 2024", 200);

            var products = new List<Product>() { product1, product2 };

            var searchResult = ProductSearcher.Search(products, "headset");

            searchResult.Should().Equal(product2);
        }

        [Test]
        public void ShouldReturnOneProduct_whenMatchedWithSingleDescription()
        {
            var product = new Product("Logitech webcam", "The best webcam in 2024", 200);

            var products = new List<Product>() { product };

            var searchResult = ProductSearcher.Search(products, "best");

            searchResult.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnMultipleProducts_whenMatchedWithMultiple()
        {
            var product = new Product("Logitech webcam", "The best webcam in 2024", 200);
            var product2 = new Product("AMD Razor Headset", "The best headset in 2024", 150);

            var products = new List<Product>() { product, product2 };

            var searchResult = ProductSearcher.Search(products, "best");

            searchResult.Should().BeEquivalentTo(new List<Product>() { product, product2 });
        }

        [Test]
        public void ShouldFilterProduct_whenPriceIsEqualToMinPrice()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 199);
            var product2 = new Product("Logitech webcam", "The best webcam in 2024", 200);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 201);


            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", 200);

            searchResult.Should().BeEquivalentTo(new List<Product>() { product2, product3 });
        }

        [Test]
        public void ShouldFilterOut_whenPriceIsBiggerThanMaxPrice()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 199);
            var product2 = new Product("Logitech webcam", "The best webcam in 2024", 401);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 201);


            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", 200, 400);

            searchResult.Should().BeEquivalentTo(new List<Product>() { product3 });
        }

        [Test]
        public void ShouldFilterIn_whenPriceIsEqualToMaxPrice()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 199);
            var product2 = new Product("Logitech webcam", "The best webcam in 2024", 400);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 201);


            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", 200, 400);

            searchResult.Should().BeEquivalentTo(new List<Product>() { product2, product3 });
        }

        [Test]
        public void ShouldThrowErrorWhenMinPriceIsSmallerThanZero()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 199);
            var products = new List<Product>() { product1 };

            var searchAction = () =>  ProductSearcher.Search(products, "best", -1);

            searchAction.Should().Throw<ApplicationException>().WithMessage("The MinPrice be bigger or equal to 0");
        }

        [Test]
        public void ShouldThrowErrorWhenMaxPriceIsSmallerThanZero()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 199);
            var products = new List<Product>() { product1 };

            var searchAction = () => ProductSearcher.Search(products, "best", 0, -4);

            searchAction.Should().Throw<ApplicationException>().WithMessage("The MaxPrice be bigger or equal to 0");
        }

        [Test]
        public void ShouldThrowError_WhenPriceDoesNotFormARange()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 199);
            var products = new List<Product>() { product1 };

            var searchAction = () => ProductSearcher.Search(products, "best", 100, 50);

            searchAction.Should().Throw<ApplicationException>().WithMessage("The MinPrice should be smaller than MaxPrice");
        }

        [Test]
        public void ShouldFilterInBasedOnDamaged()
        {
            var product1 = new Product("Logitech webcam", "The best webcam in 2024", 199);
            product1.StockLevel.QualityStatus = QualityStatus.Damaged;
            var product2 = new Product("Logitech webcam", "The best webcam in 2024", 400);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 201);


            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", null, null, QualityStatus.Damaged);

            searchResult.Should().BeEquivalentTo(new List<Product>() { product1 });
        }

        [Test]
        public void ShouldFilterInBasedOnExpired()
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
            var product1 = new Product("Xerox monitor", "The best monitor in 2024", 201);
            var product2 = new Product("AMD Process", "The best processor", 199);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 400);

            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", null, null, QualityStatus.Available, ProductSearcher.Sorting.ByNameAscending);

            searchResult.Should().Equal(new List<Product>() { product2, product3, product1 });
        }

        [Test]
        public void ShouldSortProductsByNameInDescendingOrder()
        {
            var product1 = new Product("Xerox monitor", "The best monitor in 2024", 201);
            var product2 = new Product("AMD Process", "The best processor", 199);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 400);

            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", null, null, QualityStatus.Available, ProductSearcher.Sorting.ByNameDescending);

            searchResult.Should().Equal(new List<Product>() { product1, product3, product2 });
        }

        [Test]
        public void ShouldSortProductsByPriceAscending()
        {
            var product1 = new Product("Xerox monitor", "The best monitor in 2024", 201);
            var product2 = new Product("AMD Process", "The best processor", 500);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 400);

            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", null, null, QualityStatus.Available, ProductSearcher.Sorting.ByPriceAscending);

            searchResult.Should().Equal(new List<Product>() { product1, product3, product2 });
        }

        [Test]
        public void ShouldSortProductsByPriceDescending()
        {
            var product1 = new Product("Xerox monitor", "The best monitor in 2024", 201);
            var product2 = new Product("AMD Process", "The best processor", 500);
            var product3 = new Product("Logitech webcam", "The best webcam in 2024", 400);

            var products = new List<Product>() { product1, product2, product3 };

            var searchResult = ProductSearcher.Search(products, "best", null, null, QualityStatus.Available, ProductSearcher.Sorting.ByPriceDescending);

            searchResult.Should().Equal(new List<Product>() { product2, product3, product1 });
        }
    }
}
