using MutationTestingTDD.Application.Controllers;

namespace MutationTestingTDD.Domain
{
    public interface IProductsFinder
    {
        IEnumerable<Product> Find(ProductsQueryParameters queryParameters);
    }
}