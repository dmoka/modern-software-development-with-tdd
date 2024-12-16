using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features;

public static class GetProduct
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products/{id}", async (Guid id, ISender sender) =>
            {
                var query = new Query() {Id = id};
                var response = await sender.Send(query);
                if (response.IsFailure)
                {
                    return Results.NotFound(response.Error);
                }

                return Results.Ok(response.Value);
            });
        }

        public class Query : IRequest<Result<Response>>
        {
            public Guid Id { get; set; }
        }

        public class Response
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int StockLevel { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<Response>>
        {
            public readonly WarehousingDbContext _dbContext;
            public Handler(WarehousingDbContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                var storedProduct = await _dbContext.Products.Include(p => p.StockLevel).SingleOrDefaultAsync(p => p.Id == request.Id);
                if (storedProduct == null)
                {
                    return Result<Response>.Failure(new Error("GetProduct.NotFound", "Product not found"));
                }

                var response = new Response()
                {
                    Name = storedProduct.Name,
                    Description = storedProduct.Description,
                    Price = storedProduct.Price,
                    StockLevel = storedProduct.StockLevel.Quantity
                };

                return Result<Response>.Success(response);
            }
        }
    }
}