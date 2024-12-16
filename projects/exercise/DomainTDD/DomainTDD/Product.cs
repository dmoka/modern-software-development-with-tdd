namespace DomainTDD;

public class Product
{
    public Product(string name, string desc, decimal price)
    {
        Name = name;
        Description = desc;
        Price = price;

        StockLevel = new StockLevel();
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public PickOperation LastPickOperation { get; set; }
    
    public StockLevel StockLevel { get; set; }
    
    public void Pick(int count)
    {
        LastPickOperation = PickOperation.Picked;
        StockLevel.Decrease(count);
    }
}

public class StockLevel
{
    private const int InitStockLevel = 20;

    public int Quantity { get; private set; }
    public QualityStatus QualityStatus { get; set; }

    public StockLevel()
    {
        Quantity = InitStockLevel;
    }

    public void Decrease(int quantity)
    {
        EnsureAvailableStatus();
        EnsurePickQuantityDoesntReachMaxLimit(quantity);
        EnsureAvailableStockLevel(quantity);

        Quantity -= quantity;
    }

    private void EnsureAvailableStockLevel(int quantity)
    {
        if (quantity > Quantity)
        {
            throw new ApplicationException("Cannot pick more than two stock levels");
        }
    }

    private static void EnsurePickQuantityDoesntReachMaxLimit(int quantity)
    {
        if (quantity > 10)
        {
            throw new ApplicationException("Cannot pick more than max pick limit");
        }
    }

    private void EnsureAvailableStatus()
    {
        if (QualityStatus != QualityStatus.Available)
        {
            throw new ApplicationException("Cannot pick expired product");
        }
    }
}

public enum QualityStatus
{
    Available,
    Expired,
    Damaged
}

public enum PickOperation
{
    None,
    Picked
}
