namespace MutationTestingTDD.Domain
{
    public class StockLevel : BaseEntity
    {
        public Guid ProductId { get; }

        public int Count { get; private set; }

        public StockLevel(Guid productId, int count)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            Count = count;
        }

        public void Decrease(int count)
        {
            Count -= count;
        }

        public void Increase(int count)
        {
            Count += count;
        }
    }
}
