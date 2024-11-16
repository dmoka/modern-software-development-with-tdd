using MutationTestingTDD.Application.Controllers;

namespace MutationTestingTDD.Domain
{
    public class ProductsSearcher : IProductsSearcher
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductsSearcher(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Product> Find(ProductsQueryParameters queryParameters)
        {
            var products = _unitOfWork.Products.GetAll();

            products = FilterByMinPriceIfSpecified(products, queryParameters.MinPrice);
            products = FilterByMaxPriceIfSpecified(products, queryParameters.MaxPrice);

            return products.OrderBy(p => p.Name);
        }

        private static IEnumerable<Product> FilterByMinPriceIfSpecified(IEnumerable<Product> products, decimal? minPrice)
        {
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice).ToList();
            }

            return products;
        }

        private static IEnumerable<Product> FilterByMaxPriceIfSpecified(IEnumerable<Product> products, decimal? maxPrice)
        {
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice).ToList();
            }

            return products;
        }

    }
}
