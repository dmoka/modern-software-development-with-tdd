namespace MutationTestingTDD.Domain
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        IStockLevelRepository Stocks { get; }

        Task CommitAsync();
    }
}
