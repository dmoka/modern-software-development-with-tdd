namespace MutationTestingTDD.Domain
{
    public class ProductCreatedEventHandler : IHandler<ProductCreatedEvent>
    {
        public const int InitialStockLevel = 10;
        private readonly IStockLevelRepository _repository;

        public ProductCreatedEventHandler(IStockLevelRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(ProductCreatedEvent domainEvent)
        {
            var inventoryLevel = new StockLevel(domainEvent.ProductId, InitialStockLevel);

            _repository.Create(inventoryLevel);
            return Task.CompletedTask;
        }
    }
}
