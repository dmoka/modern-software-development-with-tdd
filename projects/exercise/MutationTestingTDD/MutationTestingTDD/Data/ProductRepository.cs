using Microsoft.EntityFrameworkCore;
using MutationTestingTDD.Domain;

namespace MutationTestingTDD.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly WarehousingDbContext _dbContext;
        private readonly DbSet<Product> _entities;

        public ProductRepository(WarehousingDbContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            _dbContext = dbContext;
            _entities = dbContext.Set<Product>();
        }

        public Task<Product> GetAsync(Guid id)
        {
            return _entities.SingleOrDefaultAsync(p => p.Id == id);
        }

        public Product Create(Product entity)
        {
            var entry = _entities.Add(entity);

            return entry.Entity;
        }

        public bool Exists(string name)
        {
            return _entities.Any(p => p.Name == name);
        }

        public IEnumerable<Product> GetAll()
        {
            return _entities.ToList();
        }
    }
}
