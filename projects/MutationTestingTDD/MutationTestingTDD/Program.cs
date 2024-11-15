
using MutationTestingTDD.Data;
using MutationTestingTDD.Domain;

namespace MutationTestingTDD
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.ConfigureDbContextUsingInMemorySQLite();

            builder.Services.AddScoped<IProductsFinder, ProductsFinder>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IStockLevelRepository, StockLevelRepository>();
            builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            builder.Services.AddScoped<IHandler<ProductCreatedEvent>, ProductCreatedEventHandler>();
            builder.Services.AddScoped<IHandler<ProductPickedEvent>, ProductPickedEventHandler>();
            builder.Services.AddScoped<IHandler<ProductUnpickedEvent>, ProductUnpickedEventHandler>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
