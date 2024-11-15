using MutationTestingTDD.Application.Controllers;

namespace MutationTestingTDD.Domain
{
    public interface IProductsFinder
    {
        Task<IEnumerable<Product>> Find(ProductsQueryParameters queryParameters);
    }
}