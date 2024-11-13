using System.ComponentModel;
using Microsoft.VisualBasic;

namespace TestDoublesTDD;

public interface IProductRepository
{
    IList<Product> GetProducts();
}

public interface ExternalWarehouseService
{
    IDictionary<Guid, int> GetStockLevels();
}

public interface ILogger
{
    void LogWarning(string message);
    void LogInfo(string message);
}

public interface INotificationEmailSender
{
    void Send(string to, string subject, string body);
}

public class InventorySyncer
{
    private readonly IProductRepository _productRepo;
    private readonly ExternalWarehouseService _exernalWarehouseService;
    private readonly ILogger _logger;
    private readonly INotificationEmailSender _notificationEmailSender;


    public InventorySyncer(IProductRepository productRepo, ExternalWarehouseService exernalWarehouseService,
        ILogger logger, INotificationEmailSender notificationEmailSender)
    {
        _productRepo = productRepo;
        _exernalWarehouseService = exernalWarehouseService;
        _logger = logger;
        _notificationEmailSender = notificationEmailSender;
    }

    public void Sync()
    {
        try
        {
            var products = _productRepo.GetProducts();
            var newStockLevels = _exernalWarehouseService.GetStockLevels();

            bool anyUpdated = false;

            foreach (var product in products)
            {

                if (!newStockLevels.ContainsKey(product.Id))
                {
                    _logger.LogWarning($"No stock level data found for product {product.Id}");
                    continue;
                }

                var newStockLevel = newStockLevels[product.Id];

                if (product.UpdateStockLevel(newStockLevel))
                {
                    anyUpdated = true;
                }
            }

            if (products.Count > 0 && !anyUpdated)
            {
                _logger.LogInfo("No stock level update done");
            }
            else
            {
                _notificationEmailSender.Send("admin@tdd.com", "Inventory Sync Completed",
                    "The inventory synchronization process completed successfully.");
            }
        }
        catch (ApplicationException ex)
        {

            _notificationEmailSender.Send("admin@tdd.com", "Alert: Unexpected Sync Error",
                $"The inventory synchronization process failed unexpectedly: {ex.Message}.");
        }

    }

}