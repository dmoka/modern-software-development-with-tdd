using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features.Product;

public class GetProduct
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products/{id}", async (Guid id, WarehousingDbContext dbContext) =>
            {
                var product = await dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product is null)
                {
                    return Results.NotFound(new Error(
                        "GetProduct.NotFound",
                        $"Product with Id {id} was not found."));
                }

                var response = new Response
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price
                };

                return Results.Ok(response);
            });
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