using MutationTestingTDD.Domain;

namespace MutationTestingTDD.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WarehousingDbContext _dbContext;
        private IProductRepository _products;

        public UnitOfWork(WarehousingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IProductRepository Products => _products ??= new ProductRepository(_dbContext);



        public async Task CommitAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
