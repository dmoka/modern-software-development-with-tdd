using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Features.Product;

public class UpdateProduct
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/products/{id}", async (Guid id, Request command, WarehousingDbContext dbContext, IValidator<Request> validator) =>
            {
                command.Id = id;
                var validationResult = validator.Validate(command);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(new Error("UpdateProduct.Validation", validationResult.ToString()));
                }

                var product = await dbContext.Products.FindAsync(id);
                if (product == null)
                {
                    return Results.BadRequest(new Error("UpdateProduct.NotFound", $"Product with Id {id} was not found."));
                }

                product.Name = command.Name;
                product.Description = command.Description;
                product.Price = command.Price;

                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }
    }

    public class Request
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Id).NotEmpty();
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Description).NotEmpty();
            RuleFor(c => c.Price).GreaterThan(0);
        }
    }
} 