using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using ProductsApi.Data;
using ProductsApi.Middleware;
using ProductsApi.Services;
using ProductsApi.Validators;
using Serilog;

namespace ProductsApi.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var sqlConnectionString = configuration.GetConnectionString("ProductDBConnection") ?? throw new Exception("ProductDBConnection is missing");

        services.AddDbContext<ProductDbContext>(opt =>
        {
            opt.UseSqlite(sqlConnectionString);
        });

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ITokenService, TokenService>();
        
        services.AddSingleton(Log.Logger);
        services.AddSingleton(typeof(IAppLogger<>), typeof(SerilogLogger<>));
        
        services.AddHealthChecks()
            .AddSqlite(sqlConnectionString)
            .AddCheck("self", () => HealthCheckResult.Healthy());
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Products API", Version = "v1" });
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
