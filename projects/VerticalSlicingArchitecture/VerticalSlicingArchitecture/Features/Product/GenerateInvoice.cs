using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;
using VerticalSlicingArchitecture.Entities;

namespace VerticalSlicingArchitecture.Features.Product
{
    public static class GenerateInvoice
    {
        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("api/invoices/{id}/generate", () =>
                {
                    return Results.Ok();
                });
            }
        }

        public class Command : IRequest<Result<Guid>>
        {
            public string Id { get; set; }

        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
  
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result>
        {
            private readonly WarehousingDbContext _context;
            private readonly IValidator<Command> _validator;

            public Handler(WarehousingDbContext context, IValidator<Command> validator)
            {
                _context = context;
                _validator = validator;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                //Logic for generating invoice

                return Result.Success();
            }
        }
    }
}

