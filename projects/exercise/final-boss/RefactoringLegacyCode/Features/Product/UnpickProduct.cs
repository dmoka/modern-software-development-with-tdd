using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefactoringLegacyCode.Database;
using RefactoringLegacyCode.Shared;

namespace RefactoringLegacyCode.Features.Product
{
    public class UnpickProduct
    {
        public class Endpoint : ICarterModule
        {
            public record Command(Guid ProductId, int UnpickCount) : IRequest<Result>;

            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("api/products/{productId}/unpick", async (Guid productId, Command command, ISender sender) =>
                {
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                    {
                        return Results.BadRequest(result.Error);
                    }

                    return Results.Ok();
                });
            }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(c => c.ProductId).NotEmpty();
                    RuleFor(c => c.UnpickCount).GreaterThan(0).WithMessage("UnpickCount must be greater than 0");
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
                        return Result.Failure(new Error("UnpickProduct.Validation", validationResult.ToString()));
                    }

                    var product = await _context.Products.Include(p => p.StockLevel)
                        .SingleOrDefaultAsync(p => p.Id == request.ProductId);

                    if (product is null)
                    {
                        return Result.Failure(new Error("UnpickProduct.Validation", $"Product with id {request.ProductId} doesn't exist"));
                    }

                    var unpickResult = product.Unpick(request.UnpickCount);

                    if (unpickResult.IsFailure)
                    {
                        return Result.Failure(unpickResult.Error);
                    }

                    await _context.SaveChangesAsync();

                    return Result.Success();
                }
            }

        }
    }

}
