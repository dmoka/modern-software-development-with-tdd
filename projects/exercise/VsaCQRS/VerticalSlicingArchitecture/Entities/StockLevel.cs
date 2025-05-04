using System;
using VerticalSlicingArchitecture.Entities;
using VerticalSlicingArchitecture.Shared;

public class StockLevel
{
    private const int MaxStockLevel = 50;
    private const int MaxPickQuantityPerOperation = 10;

    public Guid Id { get; }
    public Guid ProductId { get; }
    public int Quantity { get; private set; }
    public QualityStatus QualityStatus { get; set; }
    public Product Product { get; set; }


    public static Result<StockLevel> New(Guid productId, int quantity)
    {
        if (quantity > MaxStockLevel)
        {
            return Result<StockLevel>.Failure(new Error("StockLevel.InsufficientStockLevel", "The quantity can not be more than max stock level"));
        }

        var stockLevel = new StockLevel(productId, quantity);
        return Result<StockLevel>.Success(stockLevel);
    }

    public Result Decrease(int pickCount)
    {

        if (QualityStatus != QualityStatus.Available)
        {
            return Result.Failure(new Error("PickProduct.QualityHold",
                $"Product is currently under {QualityStatus} status"));
        }

        if (pickCount > MaxPickQuantityPerOperation)
        {
            return Result.Failure(new Error("PickProduct.ExceedsMaxPick",
                $"Cannot pick more than {MaxPickQuantityPerOperation} items in a single operation"));
        }

        if (Quantity < pickCount)
        {
            return Result.Failure(
                new Error("PickProduct.InsufficientStock",
                    $"Cannot pick {pickCount} items. Only {Quantity} available"));
        }

        Quantity -= pickCount;

        return Result.Success();
    }

    public Result Increase(int quantity)
    {
        var newQuantity = Quantity + quantity;
        if (newQuantity > MaxStockLevel)
        {
            return Result.Failure(new Error("UnpickProduct.Validation", "Cannot unpick more than max stock level"));
        }

        Quantity += quantity;

        return Result.Success();
    }


    private StockLevel(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    private StockLevel() { }


}

public enum QualityStatus
{
    Available,
    Damaged,
    Expired
}