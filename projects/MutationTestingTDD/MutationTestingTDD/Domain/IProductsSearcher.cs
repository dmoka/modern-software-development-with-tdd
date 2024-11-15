using MutationTestingTDD.Application.Controllers;

namespace MutationTestingTDD.Domain
{
    public interface IProductsSearcher
    {
        IEnumerable<Product> Find(ProductsQueryParameters queryParameters);
    }
}