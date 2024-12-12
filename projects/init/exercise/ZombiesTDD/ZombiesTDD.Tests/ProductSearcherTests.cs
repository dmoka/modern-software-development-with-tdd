using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainTDD;
using FluentAssertions;

namespace ZombiesTDD.Tests
{
    public class ProductSearcherTests
    {
        [Test]
        public void ShouldReturnNoProduct_whenEmptyProductsPassed()
        {
            var products = new List<Product>();

            var result = SearchProducts(products, "WebCam");

            result.Should().BeEmpty();
        }

        [Test]
        public void ShouldReturnError_whenNullPassedAs()
        {
            var searchAction = () => SearchProducts(null, "WebCam");

            searchAction.Should().Throw<ApplicationException>().WithMessage("Products cannot be null");
        }

        [Test]
        public void ShouldReturnProduct_whenNameMatchedWithSingle()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var products = new List<Product>()
            {
                product
            };

            var result = SearchProducts(products, "WebCam");

            result.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnProduct_whenNameMatchedWithSingleFromMultiple()
        {
            var product1 = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Logictech Mouse", "The new generational mouse", 15.9m);
            var products = new List<Product>()
            {
                product1,
                product2
            };

            var result = SearchProducts(products, "Mouse");

            result.Should().Equal(product2);
        }


        [Test]
        public void ShouldReturnProduct_whenDescriptionMatchedWithSingle()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var products = new List<Product>()
            {
                product
            };

            var result = SearchProducts(products, "generational");

            result.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnProduct_whenNameMatchedWithDifferentCase()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var products = new List<Product>()
            {
                product
            };

            var result = SearchProducts(products, "WEBCAM");

            result.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnProduct_whenDescriptionMatchedWithDifferentCase()
        {
            var product = new Product("Logictech WebCam", "The new generational webcam", 20.9m);
            var products = new List<Product>()
            {
                product
            };

            var result = SearchProducts(products, "GENERATIONAL");

            result.Should().Equal(product);
        }


        [Test]
        public void ShouldReturnMultipleProducts_whenNameMatchedWithMultiple()
        {
            var product2 = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            var product1 = new Product("Logitech Mouse", "The new generational mouse", 15.9m);
            var products = new List<Product>()
            {
                product1,
                product2
            };

            var result = SearchProducts(products, "Logitech");

            result.Should().Equal(products);
        }

        [Test]
        public void ShouldFilteBasedOnDamagedProducts()
        {
            var product1 = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            product1.StockLevel.QualityStatus = QualityStatus.Damaged;
            var product2 = new Product("Logitech Mouse", "The new generational mouse", 15.9m);
            product2.StockLevel.QualityStatus = QualityStatus.Damaged;
            var product3 = new Product("Logitech Mouse v2", "The new generational mouse v2", 15.9m);
            var products = new List<Product>()
            {
                product1,
                product2,
                product3
            };

            var result = ProductSearcher.Search(products, "Logitech", QualityStatus.Damaged, Ordering.ByNameAscending);

            result.Should().Equal(new List<Product>() { product2 , product1});
        }

        [Test]
        public void ShouldSortResultsBasedNameAscending()
        {
            var product1 = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Logitech Mouse", "The new generational mouse", 15.9m);
            var product3 = new Product("Logitech Mouse v2", "The new generational mouse v2", 15.9m);
            var products = new List<Product>()
            {
                product1,
                product2,
                product3
            };

            var result = SearchProducts(products, "Logitech", Ordering.ByNameAscending);

            result.Should().Equal(new List<Product>() { product2, product3, product1});
        }

        [Test]
        public void ShouldSortResultsBasedNameDescending()
        {
            var product1 = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Logitech Mouse", "The new generational mouse", 15.9m);
            var product3 = new Product("Logitech Mouse v2", "The new generational mouse v2", 15.9m);
            var products = new List<Product>()
            {
                product1,
                product2,
                product3
            };

            var result = SearchProducts(products, "Logitech", Ordering.ByNameDescending);

            result.Should().Equal(new List<Product>() { product1, product3, product2 });
        }

        [Test]
        public void ShouldSortResultsBasedPriceAscending()
        {
            var product1 = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Logitech Mouse", "The new generational mouse", 15.9m);
            var product3 = new Product("Logitech Mouse v2", "The new generational mouse v2", 12.9m);
            var products = new List<Product>()
            {
                product1,
                product2,
                product3
            };

            var result = SearchProducts(products, "Logitech", Ordering.ByPriceAscending);

            result.Should().Equal(new List<Product>() { product3, product2, product1 });
        }

        [Test]
        public void ShouldSortResultsBasedPriceDescending()
        {
            var product1 = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Logitech Mouse", "The new generational mouse", 15.9m);
            var product3 = new Product("Logitech Mouse v2", "The new generational mouse v2", 12.9m);
            var products = new List<Product>()
            {
                product1,
                product2,
                product3
            };

            var result = SearchProducts(products, "Logitech", Ordering.ByPriceDescending);

            result.Should().Equal(new List<Product>() { product1, product2, product3 });
        }

        private static IEnumerable<Product> SearchProducts(List<Product> products, string searchTerm, QualityStatus qualityStatus = QualityStatus.Available)
        {
            var result = ProductSearcher.Search(products, searchTerm, qualityStatus, Ordering.ByNameAscending);
            return result;
        }

        private static IEnumerable<Product> SearchProducts(List<Product> products, string searchTerm, Ordering ordering)
        {
            var result = ProductSearcher.Search(products, searchTerm, QualityStatus.Available, ordering);
            return result;
        }
    }


}
