namespace TestDoublesTDD;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; }
    public decimal Price { get; set; }
    public PickStatus LastOperation { get; set; }

    public StockLevel StockLevel { get; set; }

    public Product(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
        Id = Guid.NewGuid();

        StockLevel = new StockLevel();
    }

    public void Pick(int quanitity)
    {
        LastOperation = PickStatus.Picked;

        StockLevel.Decrease(quanitity);
    }

    public void Unpick(int i)
    {
        StockLevel.Increase(i);
        var newStockLevel = StockLevel.Quantity + i;
        if (newStockLevel > 50)
        {
            throw new ApplicationException();
        }

        StockLevel.Quantity += i;
    }

    public bool UpdateStockLevel(int newStockLevel)
    {
        if (StockLevel.Quantity == newStockLevel)
        {
            return false;
        }

        StockLevel.Quantity = newStockLevel;
        if (newStockLevel == 0)
        {
            StockLevel.QualityStatus = QualityStatus.OutOfStock;
        }

        return true;
    }
}

public class StockLevel
{
    private const int InitStockLevel = 20;
    private const int MaxPickOperationCount = 10;
    private const int MaxStockLevel = 50;
    public Guid Id { get; set; }
    public int Quantity { get; set; }

    public QualityStatus QualityStatus { get; set; }


    public StockLevel()
    {
        Quantity = InitStockLevel;
        Id = Guid.NewGuid();
    }

    public void Decrease(int quanitity)
    {
        EnsurePickCountSmallerThanLimit(quanitity);
        EnsureStockIsAvailable();
        EnsureSufficientQuantity(quanitity);

        Quantity -= quanitity;
    }

    private static void EnsurePickCountSmallerThanLimit(int quanitity)
    {
        if (quanitity > MaxPickOperationCount)
        {
            throw new ApplicationException("Pick Quantity exceeds max pick count");
        }
    }

    private void EnsureSufficientQuantity(int quanitity)
    {
        if (quanitity > Quantity)
        {
            throw new ApplicationException("Insufficient stock quantity to pick");
        }
    }

    private void EnsureStockIsAvailable()
    {
        if (QualityStatus != QualityStatus.Available)
        {
            throw new ApplicationException("Can't pick damaged product");
        }
    }

    public void Increase(int quantity)
    {
        EnsureMaxStockLevelNotReached(quantity);

        Quantity += quantity;
    }

    private void EnsureMaxStockLevelNotReached(int quantity)
    {
        var newStockLevel = Quantity + quantity;

        if (newStockLevel > MaxStockLevel)
        {
            throw new ApplicationException();
        }
    }

}

public enum QualityStatus
{
    Available,
    Damaged,
    Expired,
    OutOfStock
}



public enum PickStatus
{
    None,
    Picked
}