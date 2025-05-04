using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features.Product;

public class SearchProducts
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products", async (string? searchTerm, decimal? minPrice, decimal? maxPrice, WarehousingDbContext dbContext) =>
            {
                var query = dbContext.Products.AsQueryable();

                query = ApplySearchFilter(searchTerm, minPrice, maxPrice, query);

                var products = await query.ToListAsync();
                var response = products.Select(p => new Response
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price
                }).ToList();

                return Results.Ok(response);
            });
        }

        private static IQueryable<Entities.Product> ApplySearchFilter(string? searchTerm, decimal? minPrice, decimal? maxPrice, IQueryable<Entities.Product> query)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            return query;
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