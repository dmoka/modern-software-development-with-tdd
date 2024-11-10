namespace DomainTDD;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; }
    public double Price { get; }
    public PickStatus LastPickStatus { get; set; }

    public StockLevel StockLevel { get; set; }

    public Product(string name, string description, double price)
    {
        Name = name;
        Description = description;
        Price = price;
        Id = Guid.NewGuid();

        StockLevel = new StockLevel();
    }

}

public class StockLevel
{
    public int Quantity { get; set; }

    public QualityStatus QualityStatus { get; set; }


    public StockLevel()
    {
        Quantity = 10;
    }

}

public enum QualityStatus
{
    Available
}



public enum PickStatus
{
    None,
}