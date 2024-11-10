using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features.Product
{
    public class UnpickProduct
    {
        public class Endpoint : ICarterModule
        {
            public record Command(Guid ProductId, int UnpickCount) : IRequest<Result>;

            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("api/products/{productId}/unpick", (Guid productId, Command command, ISender sender) =>
                {
                    sender.Send(command);

                    return Results.Ok();
                });
            }

            internal sealed class Handler : IRequestHandler<Command, Result>
            {
                private readonly WarehousingDbContext _context;

                public Handler(WarehousingDbContext context)
                {
                    _context = context;
                }

                public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
                { 

                    var product = await _context.Products.Include(p => p.StockLevel)
                        .SingleOrDefaultAsync(p => p.Id == request.ProductId);

                    product.Unpick(request.UnpickCount);

                    await _context.SaveChangesAsync();

                    //TODO: add check for null product. Also for pick product nedpoint

                    return Result.Success();
                }
            }

        }
    }

}
