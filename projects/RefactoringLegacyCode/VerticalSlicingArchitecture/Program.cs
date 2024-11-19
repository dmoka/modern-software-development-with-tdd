using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RefactoringLegacyCode.Data;
using RefactoringLegacyCode.Features;

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

builder.Services.AddTransient<ICustomerEmailSender, CustomerEmailSender>();
builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();

builder.Services.AddDbContext<WarehousingDbContext>(options =>
    options.UseSqlite("Data Source=WarehousingDb.db"));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddControllers(); // Add MVC services


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();

    using (var scope = app.Services.CreateScope())
    {
        SQLitePCL.Batteries_V2.Init();

        var dbContext = scope.ServiceProvider.GetRequiredService<WarehousingDbContext>();
        dbContext.Database.EnsureCreated();

        if (!dbContext.Products.Any())
        {
            dbContext.Products.Add(new Product { Id = 100, Quantity = 10, Price = 18.99m });
        }

        if (!dbContext.Orders.Any())
        {
            dbContext.Orders.Add(new OrderDetails
            {
                Id = 1,
                ProductId = 100,
                Quantity = 5,
                CustomerEmail = "customer@example.com",
                DeliveryType = DeliveryType.Express
            });
        }

        await dbContext.SaveChangesAsync();
    }

}

app.UseGlobalExceptionHandler();

app.MapControllers(); // Maps traditional MVC controllers

app.MapCarter();//This scans the current assembly, find impls for ICarderModule and calls AddRoutes

app.UseHttpsRedirection();


app.MapGet("/test", () => Results.Ok("It works!"));

app.UseAuthorization();

app.Run();

public partial class Program { }

public class CustomerEmailSender : ICustomerEmailSender
{
    public void SendEmail(StringContent content)
    {
        using var client = new HttpClient();

        client.PostAsync("https://api.sendgrid.com/v3/mail/send", content);
    }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}

// This makes the Program class public and accessible
