using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;

// Apply .net libraries
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WarehousingDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddValidatorsFromAssembly(assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandler();

app.MapCarter();//This scans the current assembly, find impls for ICarderModule and calls AddRoutes

app.UseHttpsRedirection();

app.UseAuthorization();

app.Run();

public partial class Program { } // This makes the Program class public and accessible
