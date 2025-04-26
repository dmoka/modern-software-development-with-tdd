using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features;

public static class PickProduct
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products/{id}/pick", async (Guid id, Request command, WarehousingDbContext dbContext, IValidator<Request> validator) =>
            {
                command.Id = id;

                var validationResult = validator.Validate(command);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(new Error("PickProduct.Validation", validationResult.ToString()));
                }

                var product = await dbContext.Products
                    .Include(p => p.StockLevel)
                    .SingleOrDefaultAsync(p => p.Id == command.Id);
                
                if (product == null)
                {
                    return Results.Conflict(new Error("PickProduct.NotFound", "Product not found"));
                }

                var pickResult = product.Pick(command.Count);
                if (pickResult.IsFailure)
                {
                    return Results.Conflict(pickResult.Error);
                }

                await dbContext.SaveChangesAsync();

                return Results.Ok();
            });
        }
    }

    public class Request
    {
        public Guid Id { get; set; }
        public int Count { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("The product is must be filled");
            RuleFor(c => c.Count).GreaterThan(0).WithMessage("The pick count must be bigger than 0");
        }
    }
}