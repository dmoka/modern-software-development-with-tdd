using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;
using VerticalSlicingArchitecture.Entities;
namespace VerticalSlicingArchitecture.Features.Product;

public static class CreateProduct
{

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products", async (Command Command, ISender sender) =>
            {
                var result = await sender.Send(Command);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created($"/api/products/{result.Value}", result.Value);
            });
        }
    }

    public class Command : IRequest<Result<Guid>>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
        
        public int InitialStock { get; set; }

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

    internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly WarehousingDbContext _context;

        public Handler(WarehousingDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = new Entities.Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price
            };

            _context.Products.Add(product);

            var stockLevel = new StockLevel
            {
                ProductId = product.Id,
                Quantity = request.InitialStock,
                LastUpdated = DateTime.UtcNow
            };

            _context.StockLevels.Add(stockLevel);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(product.Id);
        }
    }
}