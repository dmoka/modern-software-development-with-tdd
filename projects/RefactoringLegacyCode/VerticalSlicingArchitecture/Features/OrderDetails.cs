namespace RefactoringLegacyCode.Features;

public class OrderDetails
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public DeliveryType DeliveryType { get; set; }

    public Status Status { get; set; }

    public string CustomerEmail { get; set; }

    public void MarkProcessed()
    {
        Status = Status.Processed;
    }

}