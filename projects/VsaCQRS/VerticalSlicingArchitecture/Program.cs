using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

builder.Host.UseWolverine(options =>
{
    options.Durability.Mode = DurabilityMode.MediatorOnly;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<WarehousingDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);

var app = builder.Build();

app.UseGlobalExceptionHandler();

app.MapCarter();//This scans the current assembly, find impls for ICarderModule and calls AddRoutes

app.UseHttpsRedirection();

app.UseAuthorization();


app.Run();

public partial class Program { } // This makes the Program class public and accessible
