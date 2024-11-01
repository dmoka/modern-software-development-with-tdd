using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.Json;

public class WarehouseManagementSystem
{
    private string _connectionString = "Server=myServer;Database=WarehouseDB;User Id=myUsername;Password=myPassword;";

    public async Task ProcessOrder(int orderId)
    {
        //TODO: add edge/corner cases
        //TODO: add negative cases
        //TODO: combine presentation logic too
        // Load order details from the database
        OrderDetails orderDetails = null;
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT * FROM Orders WHERE Id = @OrderId", connection))
            {
                command.Parameters.AddWithValue("@OrderId", orderId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        orderDetails = new OrderDetails
                        {
                            OrderId = orderId,
                            ProductId = reader.GetInt32(1),
                            Quantity = reader.GetInt32(2),
                            CustomerEmail = reader.GetString(3)
                        };
                    }
                }
            }
        }

        // Check stock and reserve items (complex logic)
        if (orderDetails != null && orderDetails.Quantity > 0)
        {
            // Reserve stock
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE Products SET Quantity = Quantity - @Quantity WHERE Id = @ProductId", connection))
                {
                    command.Parameters.AddWithValue("@ProductId", orderDetails.ProductId);
                    command.Parameters.AddWithValue("@Quantity", orderDetails.Quantity);
                    command.ExecuteNonQuery();
                }
            }

            // Send confirmation email to customer (hardcoded email service)
            Console.WriteLine($"Sending email to {orderDetails.CustomerEmail} for Order {orderDetails.OrderId}...");
            // Send confirmation email to customer (inline HttpClient)
            using (var httpClient = new HttpClient())
            {
                var emailPayload = new
                {
                    to = orderDetails.CustomerEmail,
                    subject = $"Order Confirmation - Order #{orderDetails.OrderId}",
                    body = $"Dear Customer,\n\nThank you for your order #{orderDetails.OrderId}. Your order has been processed and will be delivered soon.\n\nBest Regards,\nWarehouse Team"
                };

                string emailJson = JsonSerializer.Serialize(emailPayload);
                var content = new StringContent(emailJson, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync("https://api.emailservice.com/send", content);


                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Failed to send email. Status Code: " + response.StatusCode);
                }
                else
                {
                    Console.WriteLine("Email sent successfully to " + orderDetails.CustomerEmail);
                }
            }

            // Log action to JSON file with additional structure
            decimal unitPrice = 19.99m; // Assume a fixed unit price for simplicity
            decimal totalCost = orderDetails.Quantity * unitPrice;
            DateTime orderDate = DateTime.Now;
            DateTime estimatedDeliveryDate = orderDate.AddDays(orderDetails.Quantity > 5 ? 2 : 5);
            bool isExpressDelivery = orderDetails.Quantity > 10;

            // Log action to JSON file with enhanced structure and business logic
            string fileName = $"Order_{orderDetails.OrderId}.json";
            var orderJson = new
            {
                OrderId = orderDetails.OrderId,
                ProductDetails = new
                {
                    ProductId = orderDetails.ProductId,
                    Quantity = orderDetails.Quantity,
                    UnitPrice = unitPrice,
                    TotalCost = totalCost,
                    WarehouseLocation = new
                    {
                        Aisle = "3",
                        Section = "12B",
                        Shelf = "Top"
                    }
                },
                CustomerInfo = new
                {
                    Email = orderDetails.CustomerEmail,
                    Address = new
                    {
                        Street = "Main St. 123",
                        City = "SampleCity",
                        PostalCode = "12345"
                    }
                },
                OrderDate = orderDate,
                EstimatedDeliveryDate = estimatedDeliveryDate,
                IsExpressDelivery = isExpressDelivery,
                Status = "Processed"
            };

            string jsonString = JsonSerializer.Serialize(orderJson, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fileName, jsonString);

            // Mark order as processed in database
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE Orders SET Status = 'Processed' WHERE Id = @OrderId", connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderDetails.OrderId);
                    command.ExecuteNonQuery();
                }
            }
        }
        else
        {
            Console.WriteLine("Order cannot be processed due to insufficient stock.");
        }
    }
}

public class OrderDetails
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string CustomerEmail { get; set; }
}

