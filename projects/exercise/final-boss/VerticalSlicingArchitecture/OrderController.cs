using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using System.Xml.Linq;
using RefactoringLegacyCode;

public interface IEMailSender
{
    Task SendEmailAsync(string uri, StringContent content);
}

public interface IDateTimeProvider
{
    DateTime Now { get; }
}

namespace RefactoringLegacyCode
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IEMailSender _sender;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly WarehousingDbContext _warehousingDbContext;
        private string _connectionString => _configuration.GetConnectionString("DefaultConnection");

        public OrderController(IConfiguration configuration, IEMailSender sender, IDateTimeProvider dateTimeProvider, WarehousingDbContext warehousingDbContext)
        {
            _configuration = configuration;
            _sender = sender;
            _dateTimeProvider = dateTimeProvider;
            _warehousingDbContext = warehousingDbContext;
        }

        [HttpPost("{id}/process")]
        public async Task<IActionResult> ProcessOrder(int id)
        {
            try
            {
                var orderDetails = _warehousingDbContext.Orders.SingleOrDefault(o => o.Id == id);
                if (orderDetails == null)
                {
                    return BadRequest("Order not found.");
                }

                var product = _warehousingDbContext.Products.SingleOrDefault(p => p.Id == orderDetails.ProductId);

                if ( orderDetails.Quantity > product.Quantity)
                {
                    return BadRequest("Insufficient stock to process the order.");
                }

                product.Quantity -= orderDetails.Quantity;

                await SendOrderConfirmationEmailToCustomer(orderDetails);

                var unitPrice= product.Price;
  
                var totalCost = orderDetails.Quantity * unitPrice;
                var orderDate = _dateTimeProvider.Now;
                var estimatedDeliveryDate = orderDate.AddDays(orderDetails.Quantity > 5 ? 2 : 5);

                var now = _dateTimeProvider.Now;
                var pr1 = CalculatePriority(now, orderDetails.Quantity, orderDetails.DeliveryType);
                var pr = pr1;

                LogOrderProcessingToXml(orderDetails, totalCost, orderDate, estimatedDeliveryDate, pr);

                orderDetails.MarkAsProcessed();

                var data = new
                {
                    OrderId = orderDetails.Id,
                    TotalCost = totalCost,
                    EstimatedDeliveryDate = estimatedDeliveryDate,
                    DeliveryType = orderDetails.DeliveryType
                };

                await _warehousingDbContext.SaveChangesAsync();

                return Ok(data);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private static void LogOrderProcessingToXml(OrderDetails orderDetails, decimal totalCost, DateTime orderDate,
            DateTime estimatedDeliveryDate, int pr)
        {
            var xmlDocument = new XElement("Order",
                new XElement("Id", orderDetails.Id),
                new XElement("ProductDetails",
                    new XElement("ProductId", orderDetails.ProductId),
                    new XElement("Quantity", orderDetails.Quantity),
                    new XElement("UnitPrice", 19.99m),
                    new XElement("TotalCost", totalCost)
                ),
                new XElement("CustomerInfo",
                    new XElement("Email", orderDetails.CustomerEmail),
                    new XElement("Address",
                        new XElement("Street", "Main St. 123"),
                        new XElement("City", "SampleCity"),
                        new XElement("PostalCode", "12345")
                    )
                ),
                new XElement("OrderDate", orderDate),
                new XElement("EstimatedDeliveryDate", estimatedDeliveryDate),
                new XElement("Priority", pr),
                new XElement("IsExpressDelivery", orderDetails.DeliveryType == "Express"),
                new XElement("Status", "Processed")
            );

            string xmlFileName = $"Order_{orderDetails.Id}.xml";
            xmlDocument.Save(xmlFileName);
        }

        private async Task SendOrderConfirmationEmailToCustomer(OrderDetails orderDetails)
        {
            // Send confirmation email to customer
            using (var httpClient = new HttpClient())
            {
                var emailPayload = new
                {
                    to = orderDetails.CustomerEmail,
                    subject = $"Order Confirmation - Order #{orderDetails.Id}",
                    body = $"Dear Customer,\n\nThank you for your order #{orderDetails.Id}. Your order has been processed and will be delivered soon.\n\nBest Regards,\nWarehouse Team"
                };

                string emailJson = JsonSerializer.Serialize(emailPayload);
                var content = new StringContent(emailJson, Encoding.UTF8, "application/json");

                await _sender.SendEmailAsync("https://email.myservice.com/send", content);
            }
        }

        public static int CalculatePriority(DateTime orderDate, int quantity, string deliveryType)
        {
            int pr1 = 0;
            if (quantity > 10)
            {
                // Large orders
                if (deliveryType == "Express")
                {
                    if (orderDate.Hour >= 18)
                    {
                        pr1 += 150; // Large express order placed in the evening
                    }
                    else
                    {
                        pr1 += 120; // Large express order during the day
                    }
                }
                else if (deliveryType == "SameDay")
                {
                    if (orderDate.Hour < 12)
                    {
                        pr1 += 180; // SameDay delivery placed in the morning
                    }
                    else
                    {
                        pr1 += 160; // SameDay delivery placed in the afternoon
                    }
                }
                else if (deliveryType == "Standard")
                {
                    if (quantity > 50)
                    {
                        pr1 += 100; // Very large standard delivery
                    }
                    else
                    {
                        pr1 += 80; // Regular large standard delivery
                    }
                }
            }
            else
            {
                // Small orders
                if (deliveryType == "Express")
                {
                    if (quantity > 5)
                    {
                        if (orderDate.Hour >= 18)
                        {
                            pr1 += 70; // Small express order placed in the evening
                        }
                        else
                        {
                            pr1 += 60; // Small express order during the day
                        }
                    }
                    else
                    {
                        pr1 += 50; // Very small express orders
                    }
                }
                else if (deliveryType == "SameDay")
                {
                    if (orderDate.Hour < 12)
                    {
                        pr1 += 90; // Small SameDay delivery placed in the morning
                    }
                    else
                    {
                        pr1 += 110; // Small SameDay delivery placed later in the day
                    }
                }
                else if (deliveryType == "Standard")
                {
                    if (orderDate.Hour >= 18)
                    { 
                        pr1 += 40; // Small standard order placed in the evening
                    }
                    else
                    {
                        pr1 += 20; // Small standard order during the day
                    }
                }
            }

            return pr1;
        }
    }

    public class OrderDetails
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public string DeliveryType { get; set; }

        public string CustomerEmail { get; set; }
        public string Status { get; set; }

        public void MarkAsProcessed()
        {
            Status = "Processed";
        }

    }

}