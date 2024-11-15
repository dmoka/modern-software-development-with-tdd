namespace MutationTestingTDD.Domain
{
    public class ProductCreatedEvent : IDomainEvent
    {
        public Guid ProductId { get; }

        public ProductCreatedEvent(Guid productId)
        {
            ProductId = productId;
        }
    }
}
