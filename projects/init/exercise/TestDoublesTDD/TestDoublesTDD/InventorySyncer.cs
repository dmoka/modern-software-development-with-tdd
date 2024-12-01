namespace TestDoublesTDD;

public interface IProductRepository
{
    IEnumerable<Product> GetProducts();
}

public interface IExternalWarehouseService
{
    IDictionary<Guid, int> GetStockLevels();
}

public interface IEmailSender
{
    void SendNotification(string email, string subject, string body);
}

public interface ILogger
{
    void LogWarning(string message);
    void LogInfo(string message);
}

public class InventorySyncer
{
    private readonly IProductRepository _productRespo;
    private readonly IExternalWarehouseService _externalWarehouseService;
    private readonly ILogger _logger;
    private readonly IEmailSender _emailSender;

    public InventorySyncer(IProductRepository productRespo, IExternalWarehouseService externalWarehouseService,
        ILogger logger)
    {
        _productRespo = productRespo;
        _externalWarehouseService = externalWarehouseService;
        _logger = logger;
    }


    public void Sync()
    {
        bool hasUpdates = false;

        try
        {
            var products = _productRespo.GetProducts();
            if (!products.Any())
            {
                return;
            }

            var stockLevels = _externalWarehouseService.GetStockLevels();

            foreach (var product in products)
            {
                if (!stockLevels.ContainsKey(product.Id))
                {
                    _logger.LogWarning($"No stock level data for product with id {product.Id}");
                    return;
                }

                if (product.UpdateStockLevel(stockLevels[product.Id]))
                {
                    hasUpdates = true;
                };
            }

            if (!hasUpdates)
            {
                _logger.LogInfo("No products updates");
            }
        }
        catch (Exception e)
        {
            throw new ApplicationException("Unexpected application error happened");
        }
    }
}