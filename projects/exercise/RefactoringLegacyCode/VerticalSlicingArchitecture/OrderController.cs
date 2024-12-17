using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using System.Xml.Linq;
using RefactoringLegacyCode;

namespace RefactoringLegacyCode
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string uri, StringContent content);
    }

    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IDateTimeProvider _dateTimeProvider;
        private string _connectionString => _configuration.GetConnectionString("DefaultConnection");

        public OrderController(IConfiguration configuration, IEmailSender emailSender, IDateTimeProvider dateTimeProvider)
        {
            _configuration = configuration;
            _emailSender = emailSender;
            _dateTimeProvider = dateTimeProvider;
        }

        [HttpPost("{id}/process")]
        public async Task<IActionResult> ProcessOrder(int id)
        {
            try
            {
                // Load order details from the database
                OrderDetails orderDetails = null;
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqliteCommand("SELECT * FROM Orders WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {


                                orderDetails = new OrderDetails
                                {
                                    Id = id,
                                    ProductId = reader.GetInt32(1),
                                    Quantity = reader.GetInt32(2),
                                    CustomerEmail = reader.GetString(3),
                                    DeliveryType = reader.GetString(4)
                                };
                            }
                        }
                    }
                }

                if (orderDetails != null)
                {
                    // Check if enough stock is available
                    bool stockAvailable = false;
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();
                        using (var command = new SqliteCommand("SELECT Quantity FROM Products WHERE Id = @ProductId", connection))
                        {
                            command.Parameters.AddWithValue("@ProductId", orderDetails.ProductId);
                            var productQuantity = (int)(long)command.ExecuteScalar();
                            stockAvailable = productQuantity >= orderDetails.Quantity;
                        }
                    }

                    if (!stockAvailable)
                    {
                        return BadRequest("Insufficient stock to process the order.");
                    }

                    // Reserve stock
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();
                        using (var command = new SqliteCommand("UPDATE Products SET Quantity = Quantity - @Quantity WHERE Id = @ProductId", connection))
                        {
                            command.Parameters.AddWithValue("@ProductId", orderDetails.ProductId);
                            command.Parameters.AddWithValue("@Quantity", orderDetails.Quantity);
                            command.ExecuteNonQuery();
                        }
                    }

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

                        await _emailSender.SendEmailAsync("https://api.emailservice.com/send", content);
                    }

                    // Log action to XML
                    decimal unitPrice= 0;
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();
                        using (var command = new SqliteCommand("SELECT Price FROM Products WHERE Id = @ProductId", connection))
                        {
                            command.Parameters.AddWithValue("@ProductId", orderDetails.ProductId);
                            unitPrice = Convert.ToDecimal(command.ExecuteScalar());
                        }
                    }
                    decimal totalCost = orderDetails.Quantity * unitPrice;
                    DateTime orderDate = _dateTimeProvider.Now;
                    DateTime estimatedDeliveryDate = orderDate.AddDays(orderDetails.Quantity > 5 ? 2 : 5);

                    //Calculate order priority
                    //Same day orders should have the highest priority, then express and then standard, given the same quantity and time
                    //The more the quantity the higher the priority for the same delivery type
                    var now = _dateTimeProvider.Now;
                    var pr1 = CalculatePriority(orderDetails, now);

                    var pr = pr1;

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
                        new XElement("OrderDate", _dateTimeProvider.Now),
                        new XElement("EstimatedDeliveryDate", estimatedDeliveryDate),
                        new XElement("Priority", pr),
                        new XElement("IsExpressDelivery", orderDetails.DeliveryType == "Express"),
                        new XElement("Status", "Processed")
                    );

                    string xmlFileName = $"Order_{orderDetails.Id}.xml";
                    xmlDocument.Save(xmlFileName);

                    // Mark order as processed in the database
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();
                        using (var command = new SqliteCommand("UPDATE Orders SET Status = 'Processed' WHERE Id = @Id", connection))
                        {
                            command.Parameters.AddWithValue("@Id", orderDetails.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    var data = new
                    {
                        OrderId = orderDetails.Id,
                        TotalCost = totalCost,
                        EstimatedDeliveryDate = estimatedDeliveryDate,
                        DeliveryType = orderDetails.DeliveryType
                    };

                    return Ok(data);
                }

                return BadRequest("Order not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public static int CalculatePriority(OrderDetails orderDetails, DateTime now)
        {
            int pr1 = 0;
            if (orderDetails.Quantity > 10)
            {
                // Large orders
                if (orderDetails.DeliveryType == "Express")
                {
                    if (now.Hour >= 18)
                    {
                        pr1 += 150; // Large express order placed in the evening
                    }
                    else
                    {
                        pr1 += 120; // Large express order during the day
                    }
                }
                else if (orderDetails.DeliveryType == "SameDay")
                {
                    if (now.Hour < 12)
                    {
                        pr1 += 180; // SameDay delivery placed in the morning
                    }
                    else
                    {
                        pr1 += 160; // SameDay delivery placed in the afternoon
                    }
                }
                else if (orderDetails.DeliveryType == "Standard")
                {
                    if (orderDetails.Quantity > 50)
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
                if (orderDetails.DeliveryType == "Express")
                {
                    if (orderDetails.Quantity > 5)
                    {
                        if (now.Hour >= 18)
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
                else if (orderDetails.DeliveryType == "SameDay")
                {
                    if (now.Hour < 12)
                    {
                        pr1 += 90; // Small SameDay delivery placed in the morning
                    }
                    else
                    {
                        pr1 += 110; // Small SameDay delivery placed later in the day
                    }
                }
                else if (orderDetails.DeliveryType == "Standard")
                {
                    if (now.Hour >= 18)
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

    }

}