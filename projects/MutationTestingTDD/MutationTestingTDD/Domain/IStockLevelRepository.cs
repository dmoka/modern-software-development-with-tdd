namespace MutationTestingTDD.Domain
{
    public interface IStockLevelRepository
    {
        Task<StockLevel> GetAsync(Guid productId);

        StockLevel Create(StockLevel level);
    }
}
