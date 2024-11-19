using Carter;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using VerticalSlicingArchitecture.Shared;
using System;
using FluentValidation;
using RefactoringLegacyCode.Data;

namespace RefactoringLegacyCode.Features
{
    public static class ProcessOrder
    {
        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("api/order/{orderId}/process", async (int orderId, ISender sender) =>
                {
                    var result = await sender.Send(new Command { OrderId = orderId });

                    if (result.IsFailure)
                    {
                        if (result.Error.Code == "internal")
                        {
                            return Results.Text(result.Error.Description, "text/plain", Encoding.UTF8, 500);
                        }
                        return Results.Text(result.Error.Description, "text/plain", Encoding.UTF8, 400);
                    }

                    return Results.Ok(result.Value);
                });
            }
        }

        public class Command : IRequest<Result<Response>>
        {
            public int OrderId { get; set; }
        }

        public class Response
        {
            public int OrderId { get; set; }
            public decimal TotalCost { get; set; }

            public DateTime EstimatedDeliveryDate { get; set; }

            public string DeliveryType { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<Response>>
        {
            private readonly ICustomerEmailSender _customerEmailSender;
            private readonly IDateTimeProvider _dateTimeProvider;
            private readonly WarehousingDbContext _context;


            public Handler(ICustomerEmailSender customerEmailSender, IDateTimeProvider dateTimeProvider, WarehousingDbContext context)
            {
                _customerEmailSender = customerEmailSender;
                _dateTimeProvider = dateTimeProvider;
                _context = context;
            }

            public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
            {
                OrderDetails orderDetails = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (orderDetails == null)
                {
                    return Result<Response>.Failure(new Error("", $"Order not found."));

                }

                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == orderDetails.ProductId);
                bool isStockAvailable = product.Quantity > orderDetails.Quantity;
                if (!isStockAvailable)
                {
                    return Result<Response>.Failure(new Error("", "Insufficient stock to process the order."));
                }

                product.DecreaseQuantity(orderDetails.Quantity);
                SendConfirmationEmailToCustomer(orderDetails);
                var totalCost = SaveOrderToXml(product, orderDetails, out var estimatedDeliveryDate);
                orderDetails.MarkProcessed();

                await _context.SaveChangesAsync();

                var data = new Response()
                {
                    OrderId = orderDetails.Id,
                    TotalCost = totalCost,
                    EstimatedDeliveryDate = estimatedDeliveryDate,
                    DeliveryType = orderDetails.DeliveryType.ToString()
                };

                return Result<Response>.Success(data);

            }

            private decimal SaveOrderToXml(Product product, OrderDetails orderDetails, out DateTime estimatedDeliveryDate)
            {
                var unitPrice = product.Price;
                var totalCost = orderDetails.Quantity * unitPrice;
                var orderDate = _dateTimeProvider.Now;
                estimatedDeliveryDate = orderDate.AddDays(orderDetails.Quantity > 5 ? 2 : 5);

                var pr = PriorityCalculator.CalculatePriority(_dateTimeProvider, orderDetails);

                var xmlDocument = new XElement("Order",
                    new XElement("Id", orderDetails.Id),
                    new XElement("ProductDetails",
                        new XElement("ProductId", orderDetails.ProductId),
                        new XElement("Quantity", orderDetails.Quantity),
                        new XElement("UnitPrice", unitPrice),
                        new XElement("TotalCost", totalCost)
                    ),
                    new XElement("CustomerInfo",
                        new XElement("Email", orderDetails.CustomerEmail),
                        new XElement("Address",
                            new XElement("Street", "Main St. 123"),
                            new XElement("City", "SampleCity"),
                            new XElement("PostalCode", "12345")
                        )
                    ),
                    new XElement("OrderDate", _dateTimeProvider.Now),
                    new XElement("EstimatedDeliveryDate", estimatedDeliveryDate),
                    new XElement("Priority", pr),
                    new XElement("IsExpressDelivery", orderDetails.DeliveryType == DeliveryType.Express),
                    new XElement("Status", "Processed")
                );

                string xmlFileName = $"Order_{orderDetails.Id}.xml";
                xmlDocument.Save(xmlFileName);
                return totalCost;
            }

            private void SendConfirmationEmailToCustomer(OrderDetails orderDetails)
            {
                var emailPayload = new
                {
                    to = orderDetails.CustomerEmail,
                    subject = $"Order Confirmation - Order #{orderDetails.Id}",
                    body = $"Dear Customer,\n\nThank you for your order #{orderDetails.Id}. Your order has been processed and will be delivered soon.\n\nBest Regards,\nWarehouse Team"
                };

                string emailJson = JsonSerializer.Serialize(emailPayload);
                var content = new StringContent(emailJson, Encoding.UTF8, "application/json");

                _customerEmailSender.SendEmail(content);
            }
        }

    }

}
