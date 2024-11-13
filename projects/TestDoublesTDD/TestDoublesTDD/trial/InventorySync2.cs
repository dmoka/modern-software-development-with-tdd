namespace TestDoublesTDD.trial
{

    public interface IProductRepository2
    {
        List<Product2> GetAllProducts();
        void UpdateProduct(Product2 product);
    }

    public interface IExternalWarehouseService2
    {
        Dictionary<Guid, int> GetExternalStockLevels();
    }

    public interface IStockPublisher2
    {
        void Publish(IDictionary<Guid, int> publish);
    }

    public interface INotificationService2
    {
        void SendEmail(string to, string subject, string body);

    }

    public interface ILogger2
    {
        void Log(string message);
    }

    public class InventorySyncer2(
        IProductRepository2 productRepository2,
        IExternalWarehouseService2 warehouseService2,
        IStockPublisher2 stockPublisher2,
        INotificationService2 notificationService2,
        ILogger2 logger2)
    {
        public void Sync()
        {
            try
            {
                var products = productRepository2.GetAllProducts();
                var externalStockLevels = warehouseService2.GetExternalStockLevels();
                var newStockLevelPerProduct = new Dictionary<Guid, int>();

                foreach (var product in products)
                {

                    var newStockLevel = externalStockLevels[product.Id];
                    var isUpdated = product.UpdateStockLevel(newStockLevel);

                    if (newStockLevel == 0)
                    {
                        product.MarkAsOutOfStock();
                    }

                    if (isUpdated)
                    {
                        newStockLevelPerProduct.Add(product.Id, newStockLevel);
                        productRepository2.UpdateProduct(product);
                    }
                }

                if (newStockLevelPerProduct.Count != 0)
                {
                    stockPublisher2.Publish(newStockLevelPerProduct);
                    notificationService2.SendEmail("admin@example.com", "Inventory Sync Complete",
                        "Significant inventory changes have been synchronized.");
                }
                else
                {
                    logger2.Log("Inventory sync completed with no significant changes.");
                }
            }
            catch (Exception ex)
            {
                logger2.Log($"Error during inventory synchronization: {ex.Message}");

                notificationService2.SendEmail("alert@example.com", "Inventory Sync Failure",
                    "An error occurred during inventory synchronization. Please check the logs.");
            }
        }
    }
}
