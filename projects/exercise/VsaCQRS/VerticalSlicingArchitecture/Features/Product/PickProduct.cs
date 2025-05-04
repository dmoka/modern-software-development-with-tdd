using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features.Product;

public static class PickProduct
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products/{productId}/pick", async (Guid productId, Request command, WarehousingDbContext context, IValidator<Request> validator) =>
            {
                var validationResult = validator.Validate(command);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(new Error("PickProduct.Validation", validationResult.ToString()));
                }

                var product = await context.Products
                    .Include(p => p.StockLevel)
                    .SingleOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    return Results.BadRequest(new Error("PickProduct.NoProductExist", $"The product with id {productId} doesn't exist"));
                }

                var pickResult = product.Pick(command.PickCount);

                if (pickResult.IsFailure)
                {
                    return Results.BadRequest(pickResult.Error);
                }

                await context.SaveChangesAsync();

                return Results.Ok();
            });
        }
    }

    public record Request(Guid ProductId, int PickCount);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.ProductId).NotEmpty();
            RuleFor(c => c.PickCount).GreaterThan(0);
        }
    }
} 