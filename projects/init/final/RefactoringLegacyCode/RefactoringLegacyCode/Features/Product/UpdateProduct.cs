using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefactoringLegacyCode.Database;
using RefactoringLegacyCode.Shared;

namespace RefactoringLegacyCode.Features.Product;

public class UpdateProduct
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/products/{id}", async (Guid id, Command command, ISender sender) =>
            {
                command.Id = id;
                var result = await sender.Send(command);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.NoContent();
            });
        }
    }

    public class Command : IRequest<Result>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id).NotEmpty();
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Description).NotEmpty();
            RuleFor(c => c.Price).GreaterThan(0);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly WarehousingDbContext _dbContext;
        private readonly IValidator<Command> _validator;

        public Handler(WarehousingDbContext dbContext, IValidator<Command> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure(new Error("UpdateProduct.Validation", validationResult.ToString()));
            }

            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
            {
                return Result.Failure(new Error(
                    "UpdateProduct.NotFound",
                    $"Product with Id {request.Id} was not found."));
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
} 