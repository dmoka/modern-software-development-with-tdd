using System.Collections;

namespace MutationTestingTDD.Domain
{
    public interface IProductRepository
    {
        Task<Product> GetAsync(Guid id);

        Product Create(Product product);
        
        bool Exists(string name);

        IEnumerable<Product> GetAll();
    }
}
