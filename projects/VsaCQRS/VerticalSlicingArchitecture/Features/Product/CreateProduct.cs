using Carter;
using FluentValidation;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;
using Wolverine;

namespace VerticalSlicingArchitecture.Features.Product
{
    public static class CreateProduct
    {
        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("api/products", async (Command command, IMessageBus bus) =>
                {
                    var commandResult = await bus.InvokeAsync<Result<Guid>>(command);
                    if (commandResult.IsFailure)
                    {
                        if (commandResult.Error.Code == "CreateProduct.Validation")
                        {
                            return Results.BadRequest(commandResult.Error);
                        }

                        return Results.Conflict(commandResult.Error);
                    }

                    var productId = commandResult.Value;
                    return Results.Created($"/api/products/{productId}", productId);
                });
            }
        }

        public class Command
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int InitialStock { get; set; }
        }

        public class Handler
        {
            private readonly WarehousingDbContext _context;
            private readonly IValidator<Command> _validator;

            public Handler(WarehousingDbContext context, IValidator<Command> validator)
            {
                _context = context;
                _validator = validator;
            }

            public async Task<Result<Guid>> Handle(Command command)
            {
                var validationResult = _validator.Validate(command);
                if (!validationResult.IsValid)
                {
                    return Result<Guid>.Failure(new Error("CreateProduct.Validation", validationResult.ToString()));
                }

                var product = new Entities.Product
                {
                    Name = command.Name,
                    Description = command.Description,
                    Price = command.Price
                };

                var stockLevelResult = StockLevel.New(product.Id, command.InitialStock);
                if (stockLevelResult.IsFailure)
                {
                    return Result<Guid>.Failure(stockLevelResult.Error);
                }

                product.StockLevel = stockLevelResult.Value;

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                return Result<Guid>.Success(product.Id);
            }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Name).NotEmpty();
                RuleFor(c => c.Description).NotEmpty();
                RuleFor(c => c.Price).GreaterThan(0);
            }
        }
    }
}


