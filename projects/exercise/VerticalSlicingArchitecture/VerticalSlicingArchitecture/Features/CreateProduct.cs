using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features;

public static class CreateProduct
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products", async (Request command, WarehousingDbContext dbContext, IValidator<Request> validator) =>
            {
                var validationResult = validator.Validate(command);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(new Error("CreateProduct.Validation", validationResult.ToString()));
                }

                if (dbContext.Products.Any(p => p.Name == command.Name))
                {
                    return Results.Conflict(new Error("CreateProduct.AlreadyExists", "Product with name already exists."));
                }
                
                var product = new Product(command.Name, command.Description, command.Price);
                dbContext.Products.Add(product);

                var stockLevel = StockLevel.Create(product.Id, command.InitialStockLevel);
                if (stockLevel.IsFailure)
                {
                    return Results.Conflict(stockLevel.Error);
                }

                dbContext.StockLevels.Add(stockLevel.Value);
                await dbContext.SaveChangesAsync();

                return Results.Created();
            });
        }
    }

    public class Request
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int InitialStockLevel { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(c => c.Description).NotEmpty().WithMessage("Description is required");
            RuleFor(c => c.Price).GreaterThan(0).WithMessage("Price must be greater than zero");
        }
    }
}

