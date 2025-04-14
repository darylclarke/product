using Microsoft.EntityFrameworkCore;
using ProductsApi.Data;
using ProductsApi.DTOs;
using ProductsApi.Exceptions;
using ProductsApi.Models;

namespace ProductsApi.Services;

public class ProductService(ProductDbContext context, IAppLogger<ProductService> logger)
    : IProductService
{
    public async Task<Product> CreateProductAsync(CreateProductDto productDto)
    {
        logger.LogInformation("Creating new product with name: {ProductName}", productDto.Name);
        
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Colour = productDto.Colour,
            CreatedAt = DateTime.UtcNow
        };
        
        await context.Products.AddAsync(product);
        
        var savedCount = await context.SaveChangesAsync();
        
        if (savedCount > 0)
        {
            logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
            return product;
        }
        
        logger.LogError("Failed to create product: {ProductName}", productDto.Name);
        throw new ProductNotCreatedException(productDto.Name);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        logger.LogInformation("Retrieving all products");
        return await context.Products.ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByColourAsync(string colour)
    {
        logger.LogInformation("Retrieving products by colour: {Colour}", colour);
        
        return await context.Products
            .Where(p => p.Colour.ToLower() == colour.ToLower())
            .ToListAsync();
    }
}
