namespace MutationTestingTDD.Domain
{
    public class Product : BaseEntity
    {
        public string Name { get; }
        public ProductCategory Category { get; }
        public string Description { get; }
        public decimal Price { get; }
        public SaleState SaleState { get; }
        public PickState LastPickState { get; private set; }

        public Product(string name, ProductCategory category, decimal price, SaleState saleState)
        {
            Id = Guid.NewGuid();
            Category = category;
            Price = price;
            SaleState = saleState;
            Name = name;
            SaleState = saleState;
            LastPickState = PickState.New;

            RaiseDomainEvent(new ProductCreatedEvent(Id));

        }

        public void Pick(int count)
        {
            LastPickState = PickState.Picked;

            RaiseDomainEvent(new ProductPickedEvent(Id, count));
        }

        public void Unpick(int count)
        {
            LastPickState = PickState.Unpicked;

            RaiseDomainEvent(new ProductUnpickedEvent(Id, count));

        }
    }

    public enum PickState
    {
        New,
        Picked,
        Unpicked
    }

    public enum SaleState
    {
        NoSale,
        OnSale,
    }
}
