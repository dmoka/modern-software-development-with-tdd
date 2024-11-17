﻿using Microsoft.AspNetCore.Mvc.Testing;
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

namespace RefactoringLegacyCode.Tests.Shared
{
    public class InMemoryServer : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly HttpClient _client;

        public string ConnectionString => _connection.ConnectionString;

        public InMemoryServer()
        {
            // Create and open the SQLite in-memory database
            SQLitePCL.Batteries.Init();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE Orders (
                Id INTEGER PRIMARY KEY,
                ProductId INTEGER,
                Quantity INTEGER,
                CustomerEmail TEXT
            );

            CREATE TABLE Products (
                Id INTEGER PRIMARY KEY,
                Quantity INTEGER
            );

            INSERT INTO Orders (Id, ProductId, Quantity, CustomerEmail)
            VALUES (1, 100, 5, 'customer@example.com');

            INSERT INTO Products (Id, Quantity)
            VALUES (100, 10);
        ";
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            _connection.Close();
        }

        public HttpClient CreateClient()
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

                        services.Configure<HttpsRedirectionOptions>(options =>
                        {
                            options.HttpsPort = 443;  // Set a dummy port or disable redirection
                        });
                    });
                });

            return factory.CreateClient();
        }
    }

}
