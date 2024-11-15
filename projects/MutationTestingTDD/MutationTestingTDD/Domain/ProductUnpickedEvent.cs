namespace MutationTestingTDD.Domain
{
    public class ProductUnpickedEvent : IDomainEvent
    {
        public Guid ProductId { get; }
        public int Count { get; }

        public ProductUnpickedEvent(Guid productId, int count)
        {
            ProductId = productId;
            Count = count;
        }
    }
}
