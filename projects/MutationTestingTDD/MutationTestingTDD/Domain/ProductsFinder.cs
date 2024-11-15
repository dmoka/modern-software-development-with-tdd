using MutationTestingTDD.Application.Controllers;

namespace MutationTestingTDD.Domain
{
    public class ProductsFinder : IProductsFinder
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductsFinder(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Product> Find(ProductsQueryParameters queryParameters)
        {
            var products = _unitOfWork.Products.GetAll();

            products = FilterByMaxPriceIfSpecified(products, queryParameters.MaxPrice);

            return products.OrderBy(p => p.Name);
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
