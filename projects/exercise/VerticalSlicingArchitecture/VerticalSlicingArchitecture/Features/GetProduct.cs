using Carter;
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
            app.MapGet("api/products/{id}", async (Guid id, WarehousingDbContext dbContext) =>
            {
                var storedProduct = await dbContext.Products
                    .Include(p => p.StockLevel)
                    .SingleOrDefaultAsync(p => p.Id == id);
                
                if (storedProduct == null)
                {
                    return Results.NotFound(new Error("GetProduct.NotFound", "Product not found"));
                }

                var response = new Response
                {
                    Name = storedProduct.Name,
                    Description = storedProduct.Description,
                    Price = storedProduct.Price,
                    StockLevel = storedProduct.StockLevel.Quantity
                };

                return Results.Ok(response);
            });
        }
    }

    public class Response
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockLevel { get; set; }
    }
}