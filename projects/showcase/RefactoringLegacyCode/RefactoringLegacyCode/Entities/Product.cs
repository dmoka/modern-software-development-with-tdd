using RefactoringLegacyCode.Shared;

namespace RefactoringLegacyCode.Entities
{
    public enum LastOperation
    {
        None,
        Picked,
        Unpicked
    }

    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
        public LastOperation LastOperation { get; set; }

        public Product()
        {
            Id = Guid.NewGuid();
        }

        public StockLevel StockLevel { get; set; }

        public Result Pick(int pickCount)
        {
            LastOperation = LastOperation.Picked;

            return StockLevel.Decrease(pickCount);
        }

        public Result Unpick(int quantity)
        {
            LastOperation = LastOperation.Unpicked;

            return StockLevel.Increase(quantity);
        }
    }
}
