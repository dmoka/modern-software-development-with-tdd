using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Database;
using VerticalSlicingArchitecture.Middlewares;

//TODO:
//-add global error handling
//-add logging/auth - https://juliocasal.com/blog/Dont-Unit-Test-Your-AspNetCore-API
// pipeline behaviours for cruss cutting concerns!!!
// - Add CLI tool with TDD
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
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);
//builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
//builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingPipelineBehavior<,>));
//builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(QueryCachingPipelineBehavior<,>));
//builder.Services.AddMemoryCache();
//builder.Services.AddSingleton<ICacheService, MemoryCacheService>();


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
