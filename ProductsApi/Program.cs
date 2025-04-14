using FluentValidation;
using ProductsApi.DTOs;
using ProductsApi.Extensions;
using ProductsApi.Services;
using Serilog;
using Microsoft.AspNetCore.Http.Json;
using ProductsApi.Models;
using System.IO;
using ProductsApi.Filters;

var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logsDirectory))
{
    Directory.CreateDirectory(logsDirectory);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logsDirectory, "productsapi.txt"), rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .CreateLogger();

try
{
    Log.Information("Starting ProductsApi");
    Log.Information("Logs are being written to {LogsDirectory}", logsDirectory);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");

    app.MapPost("/get-token", (ITokenService tokenService) => Results.Ok(new
        {
            token = tokenService.CreateToken()
        }))
        .AllowAnonymous()
        .WithName("GetToken")
        .Produces(StatusCodes.Status200OK)
        .WithOpenApi();

    app.MapPost("/api/products", async (IProductService service, CreateProductDto productDto) =>
        {
            var result = await service.CreateProductAsync(productDto);
            return Results.Created($"/api/products/{result.Id}", result);
        })
        .WithName("CreateProduct")
        .WithOpenApi()
        .WithValidation<CreateProductDto>()
        .Produces<Product>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

    app.MapGet("/api/products", async (IProductService service, string? color = null) =>
        {
            if (string.IsNullOrEmpty(color))
                return Results.Ok(await service.GetAllProductsAsync());
            
            return Results.Ok(await service.GetProductsByColourAsync(color));
        })
        .WithName("GetProducts")
        .WithOpenApi()
        .Produces<List<Product>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
