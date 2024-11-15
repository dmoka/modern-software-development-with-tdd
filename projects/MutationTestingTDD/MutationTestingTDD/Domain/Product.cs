namespace MutationTestingTDD.Domain
{

    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
        public PickStatus PickStatus { get; set; }

        public Product(string name, string description, decimal price)
        {
            Name = name;
            Description = description;
            Price = price;
            Id = Guid.NewGuid();

            StockLevel = new StockLevel();
        }

        public StockLevel StockLevel { get; set; }

        public void Pick(int quanitity)
        {
            PickStatus = PickStatus.Picked;

            StockLevel.Decrease(quanitity);
        }

        /*public void Unpick(int i)
        {
            StockLevel.Increase(i);
            var newStockLevel = StockLevel.Quantity + i;
            if (newStockLevel > 50)
            {
                throw new ApplicationException();
            }

            StockLevel.Quantity += i;
        }*/
    }

    public enum QualityStatus
    {
        Available,
        Damaged,
        Expired
    }

    public enum PickStatus
    {
        None,
        Picked,
        Unpicked
    }
}

