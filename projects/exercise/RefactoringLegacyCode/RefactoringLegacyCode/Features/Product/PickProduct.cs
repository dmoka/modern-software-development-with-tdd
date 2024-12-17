using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefactoringLegacyCode.Database;
using RefactoringLegacyCode.Shared;

namespace RefactoringLegacyCode.Features.Product;

public static class PickProduct
{
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

    public record Command(Guid ProductId, int PickCount) : IRequest<Result>;


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

            var product = await _context.Products
                .Include(p => p.StockLevel)
                .SingleOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                return Result.Failure(new Error("PickProduct.NoProductExist", $"The product with id {request.ProductId} doesn't exist"));
            }


            var pickResult =  product.Pick(request.PickCount);

            if (pickResult.IsFailure)
            {
                return Result.Failure(pickResult.Error);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
} 