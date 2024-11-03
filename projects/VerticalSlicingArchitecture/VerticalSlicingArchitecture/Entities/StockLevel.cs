using System;
using VerticalSlicingArchitecture.Shared;

public class StockLevel
{
    private const int MaxStockLevel = 50;

    public Guid Id { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }
    public DateTime LastUpdated { get; }


    public static Result<StockLevel> New(Guid productId, int quantity, DateTime lastUpdated)
    {
        if (quantity > MaxStockLevel)
        {
            return Result<StockLevel>.Failure(new Error("CreateArticle.Validation", "The quantity can not be more than max stock level"));

        }
        var stockLevel = new StockLevel(productId, quantity, lastUpdated);

       return Result<StockLevel>.Success(stockLevel);
    }

    private StockLevel(Guid productId, int quantity, DateTime lastUpdated)
    {
        ProductId = productId;
        Quantity = quantity;
        LastUpdated = lastUpdated;
    }

    private StockLevel() { }

}