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
        public void ShouldReturnNoProduct_WhenNoProductFound() 
        {
            var searcher = new ProductSearcher();

            var products = searcher.Search(new List<Product>(), "dummy", QualityStatus.Available);

            products.Should().BeEmpty();
        }

        [Test]
        public void ShouldThrowError_WhenNullSpecifiedAsInput()
        {
            var searcher = new ProductSearcher();

            var searchAction = () => searcher.Search(null, "dummy", QualityStatus.Available);

            searchAction.Should().Throw<ApplicationException>("Null cannot be specified as input");
        }

        [Test]
        public void ShouldReturnProduct_WhenNameMatchedWithSingle()
        {
            var searcher = new ProductSearcher();
            var product = new Product("Logitech WebCam", "The new generational webcam", 20.9m);

            var products = new List<Product> { product };

            var foundProduct = searcher.Search(products, "Logitech", QualityStatus.Available);

            foundProduct.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnProduct_WhenNameMatchedWithSingleFromMultiple()
        {
            var searcher = new ProductSearcher();
            var product = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Acer WebCam", "The new generational webcam", 20.9m);

            var products = new List<Product> { product, product2 };

            var foundProduct = searcher.Search(products, "Logitech", QualityStatus.Available);

            foundProduct.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnProduct_WhenNameMatchedWithDifferentCase()
        {
            var searcher = new ProductSearcher();
            var product = new Product("Logitech WebCam", "The new generational webcam", 20.9m);

            var products = new List<Product> { product };

            var foundProduct = searcher.Search(products, "LOGITECH", QualityStatus.Available);

            foundProduct.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnProduct_WhenDescMatchedWithSingle()
        {
            var searcher = new ProductSearcher();
            var product = new Product("Logitech WebCam", "The new generational webcam", 20.9m);

            var products = new List<Product> { product };

            var foundProduct = searcher.Search(products, "generational", QualityStatus.Available);

            foundProduct.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnProduct_WhenDescMatchedWithSingleFromMultiple()
        {
            var searcher = new ProductSearcher();
            var product = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Acer WebCam", "Best webcam in 2025", 20.9m);

            var products = new List<Product> { product, product2 };

            var foundProduct = searcher.Search(products, "generational", QualityStatus.Available);

            foundProduct.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnProduct_WhenDescMatchedWithDifferentCase()
        {
            var searcher = new ProductSearcher();
            var product = new Product("Logitech WebCam", "The new generational webcam", 20.9m);

            var products = new List<Product> { product };

            var foundProduct = searcher.Search(products, "GENERATIONAL", QualityStatus.Available);

            foundProduct.Should().Equal(product);
        }

        [Test]
        public void ShouldReturnMultipleProduct_whenMatchedWithMultiple()
        {
            var searcher = new ProductSearcher();
            var product = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            var product2 = new Product("Acer WebCam", "Best webcam in 2025", 20.9m);

            var products = new List<Product> { product, product2 };

            var foundProducts = searcher.Search(products, "webcam", QualityStatus.Available);

            foundProducts.Should().Equal(new List<Product>() {product, product2});
        }

        [Test]
        public void ShouldFilterBasedOnDamagedProducts()
        {
            var searcher = new ProductSearcher();
            var product = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            product.StockLevel.QualityStatus = QualityStatus.Damaged;
            var product2 = new Product("Acer WebCam", "Best webcam in 2025", 20.9m);

            var products = new List<Product> { product, product2 };

            var searchResult = searcher.Search(products, "webcam", QualityStatus.Damaged);
            searchResult.Should().Equal(new List<Product>() {product});
        }


        [Test]
        public void ShouldFilterBasedOnExpiredProducts()
        {
            var searcher = new ProductSearcher();
            var product = new Product("Logitech WebCam", "The new generational webcam", 20.9m);
            product.StockLevel.QualityStatus = QualityStatus.Expired;
            var product2 = new Product("Acer WebCam", "Best webcam in 2025", 20.9m);

            var products = new List<Product> { product, product2 };

            var searchResult = searcher.Search(products, "webcam", QualityStatus.Expired);
            searchResult.Should().Equal(new List<Product>() { product });
        }

    }
}
