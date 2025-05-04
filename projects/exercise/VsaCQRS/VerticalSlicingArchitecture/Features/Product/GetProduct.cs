using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;
using Wolverine;

namespace VerticalSlicingArchitecture.Features.Product;

public class GetProduct
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products/{id}", async (Guid id, IMessageBus bus) =>
            {
                var response = await bus.InvokeAsync<Response>(new Query { Id = id });
                if (response == null)
                {
                    return Results.NotFound(new Error(
                        "GetProduct.NotFound",
                        $"Product with Id {id} was not found."));
                }

                return Results.Ok(response);
            });
        }
    }

    public class Query
    {
        public Guid Id { get; set; }
    }

    public class Handler
    {
        private readonly WarehousingDbContext _dbContext;

        public Handler(WarehousingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response?> Handle(Query query)
        {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == query.Id);

            if (product is null)
            {
                return null;
            }

            return new Response
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };
        }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
} 