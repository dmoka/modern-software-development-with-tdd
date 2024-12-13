using Carter;
using FluentValidation;
using MediatR;
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
            app.MapPost("api/products/{id}/pick", async (Guid id, Command command, ISender sender) =>
            {
                command.Id = id;

                var result = await sender.Send(command);
                if (result.IsFailure)
                {
                    if (result.Error.Code == "PickProduct.Validation")
                    {
                        return Results.BadRequest(result.Error);
                    }
                    return Results.Conflict(result.Error);
                }

                return Results.Ok();
            });
        }

        public class Command : IRequest<Result>
        {
            public Guid Id { get; set; }
            public int Count { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Id).NotEmpty().WithMessage("The product is must be filled");
                RuleFor(c => c.Count).GreaterThan(0).WithMessage("The pick count must be bigger than 0");
            }
        }

        public class Handler : IRequestHandler<Command, Result>
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
                    return Result.Failure(new Error("PickProduct.Validation", validationResult.ToString()));
                }

                var product = _dbContext.Products.Include(p => p.StockLevel).SingleOrDefault(p => p.Id == request.Id);
                if (product == null)
                {
                    return Result.Failure(new Error("PickProduct.NotFound", "Product not found"));
                }

                var pickResult = product.Pick(request.Count);
                if (pickResult.IsFailure)
                {
                    return Result.Failure(pickResult.Error);
                }

                await _dbContext.SaveChangesAsync();

                return Result.Success();
            }
        }
    }
}