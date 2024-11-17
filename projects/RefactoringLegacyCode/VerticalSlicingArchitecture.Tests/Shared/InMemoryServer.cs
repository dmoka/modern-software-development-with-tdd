using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using FluentAssertions.Common;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Data.SqlClient;
using Moq.Protected;
using Moq;
using System.Net;
using System.Net.Http;

namespace RefactoringLegacyCode.Tests.Shared
{
    public class InMemoryServer : IDisposable
    {
        private readonly SqliteConnection _connection;
        private HttpClient _client;
        private Mock<ICustomerEmailSender> _customerEmailSender;
        private Mock<IDateTimeProvider> _dateTimeProvider;

        public string ConnectionString => _connection.ConnectionString;

        public InMemoryServer()
        {
            // Create and open the SQLite in-memory database
            SQLitePCL.Batteries.Init();

            var uniqueDbId = Guid.NewGuid().ToString();
            var dataSource = $"DataSource=file:{uniqueDbId}?mode=memory&cache=shared";
            _connection = new SqliteConnection(dataSource);
            _connection.Open();
            InitializeDatabase();
            Setup();
        }

        private void InitializeDatabase()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE Orders (
                Id INTEGER PRIMARY KEY,
                ProductId INTEGER,
                Quantity INTEGER,
                CustomerEmail TEXT,
                DeliveryType TEXT,
                Status TEXT DEFAULT 'New'
            );

            CREATE TABLE Products (
                Id INTEGER PRIMARY KEY,
                Quantity INTEGER,
                Price NUMERIC(10, 2)
            );

            INSERT INTO Orders (Id, ProductId, Quantity, CustomerEmail, DeliveryType)
            VALUES (1, 100, 5, 'customer@example.com', 'Express');

            INSERT INTO Products (Id, Quantity, Price)
            VALUES (100, 10, 18.99);
        ";
            command.ExecuteNonQuery();

            var orders = GetOrders();
        }

        public void Dispose()
        {
            _connection.Close();
        }

        public Mock<ICustomerEmailSender> EmailSender() => _customerEmailSender;
        public Mock<IDateTimeProvider> DateTimeProvider() => _dateTimeProvider;
        public HttpClient Client() => _client;

        public void Setup()
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureLogging(logging =>
                    {
                        // Set minimal logging level
                        logging.ClearProviders(); // Remove default providers
                        logging.AddConsole(); // Optional: Add console logging if needed
                        logging.SetMinimumLevel(LogLevel.Warning); // Show only warnings and errors
                    });

                    // Disable HTTPS redirection

                    builder.ConfigureServices(services =>
                    {

                        // Override the connection string for tests
                        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string>
                            {
                            { "ConnectionStrings:DefaultConnection", _connection.ConnectionString }
                            })
                            .Build());

                        _customerEmailSender = new Mock<ICustomerEmailSender>();
                        services.AddSingleton(_customerEmailSender.Object);

                        _dateTimeProvider = new Mock<IDateTimeProvider>();
                        services.AddSingleton(_dateTimeProvider.Object);

                        services.Configure<HttpsRedirectionOptions>(options =>
                        {
                            options.HttpsPort = 443;  // Set a dummy port or disable redirection
                        });
                    });
                });

            var client =  factory.CreateClient();
            _client = client;
        }

        public List<string> GetOrders()
        {
            var orders = new List<string>();

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Id, ProductId, Quantity, CustomerEmail FROM Orders";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var orderDetails = $"Id: {reader.GetInt32(0)}, ProductId: {reader.GetInt32(1)}, Quantity: {reader.GetInt32(2)}, Email: {reader.GetString(3)}";
                orders.Add(orderDetails);
            }

            return orders;
        }
    }

}
