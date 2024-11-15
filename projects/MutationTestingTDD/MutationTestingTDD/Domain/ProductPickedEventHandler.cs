namespace MutationTestingTDD.Domain
{
    public class ProductUnpickedEventHandler : IHandler<ProductUnpickedEvent>
    {

        private readonly IStockLevelRepository _repository;

        public ProductUnpickedEventHandler(IStockLevelRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(ProductUnpickedEvent domainEvent)
        {
            var stockLevel = await _repository.GetAsync(domainEvent.ProductId);

            var newStockLevel = stockLevel.Count + domainEvent.Count;
            if (newStockLevel > 50)
            {
                throw new ApplicationException();
            }

            stockLevel.Increase(domainEvent.Count);
        }

    }
}
