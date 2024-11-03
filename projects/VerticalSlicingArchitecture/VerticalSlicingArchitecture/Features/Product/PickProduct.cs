using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features.Product;

public static class PickProduct
{
    public record Command(Guid ProductId, int PickCount) : IRequest<Result>;
    

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products/{productId}/pick", async (Guid productId, Command command, ISender sender) =>
            {
                var result = await sender.Send(command);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Ok();
            });
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ProductId).NotEmpty();
            RuleFor(c => c.PickCount).GreaterThan(0);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly WarehousingDbContext _context;
        private readonly IValidator<Command> _validator;

        public Handler(WarehousingDbContext context, IValidator<Command> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure(new Error("PickProduct.Validation", validationResult.ToString()));
            }

            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(sl => sl.ProductId == request.ProductId, cancellationToken);

            if (stockLevel == null)
            {
                return Result.Failure(new Error("PickProduct.NotFound", "Stock level not found for the product"));
            }

            if (stockLevel.Quantity < request.PickCount)
            {
                return Result.Failure(
                    new Error("PickProduct.InsufficientStock", 
                    $"Cannot pick {request.PickCount} items. Only {stockLevel.Quantity} available"));
            }

            stockLevel.UpdateQuantity(stockLevel.Quantity - request.PickCount);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
} 