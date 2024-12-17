using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefactoringLegacyCode.Database;
using RefactoringLegacyCode.Shared;

namespace RefactoringLegacyCode.Features.Product;

public class SearchProducts
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products", async (string? searchTerm, decimal? minPrice, decimal? maxPrice, ISender sender) =>
            {
                var query = new Query 
                { 
                    SearchTerm = searchTerm,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };
                var result = await sender.Send(query);
                return Results.Ok(result.Value);
            });
        }
    }

    public class Query : IRequest<Result<List<Response>>>
    {
        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, Result<List<Response>>>
    {
        private readonly WarehousingDbContext _dbContext;

        public Handler(WarehousingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _dbContext.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) || 
                    p.Description.ToLower().Contains(searchTerm));
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= request.MaxPrice.Value);
            }

            var products = await query
                .Select(p => new Response
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price
                })
                .ToListAsync(cancellationToken);

            return Result<List<Response>>.Success(products);
        }
    }
} 