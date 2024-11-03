using System;
using VerticalSlicingArchitecture.Shared;

public class StockLevel
{
    private const int MaxStockLevel = 50;

    public Guid Id { get; }
    public Guid ProductId { get; }
    public int Quantity { get; private set; }
    public DateTime LastUpdated { get; private set; }

    public static Result<StockLevel> New(Guid productId, int quantity, DateTime lastUpdated)
    {
        if (quantity > MaxStockLevel)
        {
            return Result<StockLevel>.Failure(new Error("CreateArticle.Validation", "The quantity can not be more than max stock level"));
        }
        var stockLevel = new StockLevel(productId, quantity, lastUpdated);
        return Result<StockLevel>.Success(stockLevel);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity > MaxStockLevel)
        {
            throw new InvalidOperationException($"Quantity cannot exceed {MaxStockLevel}");
        }
        if (newQuantity < 0)
        {
            throw new InvalidOperationException("Quantity cannot be negative");
        }
        
        Quantity = newQuantity;
        LastUpdated = DateTime.UtcNow;
    }

    private StockLevel(Guid productId, int quantity, DateTime lastUpdated)
    {
        ProductId = productId;
        Quantity = quantity;
        LastUpdated = lastUpdated;
    }

    private StockLevel() { }
}