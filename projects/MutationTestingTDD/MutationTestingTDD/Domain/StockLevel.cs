namespace MutationTestingTDD.Domain;

public class StockLevel
{
    private const int InitStockLevel = 20;
    private const int MaxPickOperationCount = 30;
    private const int MaxStockLevel = 50;
    public Guid Id { get; set; }
    public int Quantity { get; set; }

    public QualityStatus QualityStatus { get; set; }

    public Guid ProductId { get; }
    public Product Product { get; set; }


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

   /* public void Increase(int quantity)
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
    }*/

}