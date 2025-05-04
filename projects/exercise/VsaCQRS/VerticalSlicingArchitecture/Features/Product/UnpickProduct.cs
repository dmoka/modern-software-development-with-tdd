using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features.Product;

public class UnpickProduct
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products/{productId}/unpick", async (Guid productId, Request command, WarehousingDbContext context, IValidator<Request> validator) =>
            {
                var validationResult = validator.Validate(command);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(new Error("UnpickProduct.Validation", validationResult.ToString()));
                }

                var product = await context.Products.Include(p => p.StockLevel)
                    .SingleOrDefaultAsync(p => p.Id == productId);

                if (product is null)
                {
                    return Results.Conflict(new Error("UnpickProduct.Validation", $"Product with id {productId} doesn't exist"));
                }

                var unpickResult = product.Unpick(command.UnpickCount);

                if (unpickResult.IsFailure)
                {
                    return Results.Conflict(unpickResult.Error);
                }

                await context.SaveChangesAsync();

                return Results.Ok();
            });
        }
    }

    public record Request(Guid ProductId, int UnpickCount);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.ProductId).NotEmpty();
            RuleFor(c => c.UnpickCount).GreaterThan(0).WithMessage("UnpickCount must be greater than 0");
        }
    }
}
