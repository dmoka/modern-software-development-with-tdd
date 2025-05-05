using Carter;
using FluentValidation;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features.Product
{
    public static class CreateProduct
    {
        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("api/products", async (Command command, WarehousingDbContext context, IValidator<Command> validator) =>
                {
                    var validationResult = validator.Validate(command);
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(new Error("CreateProduct.Validation", validationResult.ToString()));
                    }

                    var product = new Entities.Product
                    {
                        Name = command.Name,
                        Description = command.Description,
                        Price = command.Price
                    };

                    var stockLevelResult = StockLevel.New(product.Id, command.InitialStock);
                    if (stockLevelResult.IsFailure)
                    {
                        return Results.Conflict(stockLevelResult.Error);
                    }

                    product.StockLevel = stockLevelResult.Value;

                    await context.Products.AddAsync(product);
                    await context.SaveChangesAsync();

                    return Results.Created($"/api/products/{product.Id}", product.Id);
                });
            }
        }

        public class Command
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int InitialStock { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Name).NotEmpty();
                RuleFor(c => c.Description).NotEmpty();
                RuleFor(c => c.Price).GreaterThan(0);
            }
        }
    }
}


