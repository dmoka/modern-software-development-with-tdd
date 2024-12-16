using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features;

public static class CreateProduct
{
    public class Endpoint: ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products", async (Command command, ISender sender) =>
            {
                var result = await sender.Send(command);
                if (result.IsFailure)
                {
                    if (result.Error.Code.Equals("CreateProduct.Validation"))
                    {
                        return Results.BadRequest(result.Error);
                    }

                    return Results.Conflict(result.Error);
                }

                return Results.Created();
            });
        }

        public class Command : IRequest<Result>
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int InitialStockLevel { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Name).NotEmpty().WithMessage("Name is required");
                RuleFor(c => c.Description).NotEmpty().WithMessage("Description is required");
                RuleFor(c => c.Price).GreaterThan(0).WithMessage("Price must be greater than zero");
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
                    return Result.Failure(new Error("CreateProduct.Validation", validationResult.ToString()));
                }

                if (_dbContext.Products.Any(p => p.Name == request.Name))
                {
                    return Result.Failure(new Error("CreateProduct.AlreadyExists", "Product with name already exists."));
                }
                
                var product = new Product(request.Name, request.Description, request.Price);
                _dbContext.Products.Add(product);

                var stockLevel = StockLevel.Create(product.Id, request.InitialStockLevel);
                if (stockLevel.IsFailure)
                {
                    return Result.Failure(stockLevel.Error);
                }

                _dbContext.StockLevels.Add(stockLevel.Value);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }
    }
}

