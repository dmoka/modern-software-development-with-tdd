using Carter;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

//TODO:
//-add global error handling
//-add logging/auth - https://juliocasal.com/blog/Dont-Unit-Test-Your-AspNetCore-API
// pipeline behaviours for cruss cutting concerns!!!
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*builder.Services.AddDbContext<WarehousingDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Database")));*/

var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddControllers(); // Add MVC services

// Initialize SQLitePCL
Batteries_V2.Init();
DbInitializer.InitializeDatabase();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers(); // Maps traditional MVC controllers

app.MapCarter();//This scans the current assembly, find impls for ICarderModule and calls AddRoutes

app.UseHttpsRedirection();

app.UseAuthorization();

app.Run();

public partial class Program { } // This makes the Program class public and accessible

public class DbInitializer
{
    public static void InitializeDatabase()
    {
        using var connection = new SqliteConnection("Data Source=warehousing.db");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Orders (
            Id INTEGER PRIMARY KEY,
            ProductId INTEGER,
            Quantity INTEGER,
            CustomerEmail TEXT,
            DeliveryType TEXT,
            Status TEXT DEFAULT 'New'
        );

        CREATE TABLE IF NOT EXISTS Products (
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
    }

}

