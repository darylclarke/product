using Microsoft.EntityFrameworkCore;
using ProductsApi.Data;
using ProductsApi.DTOs;
using ProductsApi.Exceptions;
using ProductsApi.Models;
using ProductsApi.Services;
using Moq;

namespace ProductsApi.Tests.Services;

public class ProductServiceTests
{
    private readonly DbContextOptions<ProductDbContext> _options = new DbContextOptionsBuilder<ProductDbContext>()
        .UseInMemoryDatabase(databaseName: $"TestProductDb{Guid.NewGuid()}")
        .Options;
    private readonly Mock<IAppLogger<ProductService>> _mockLogger = new();

    [Fact]
    public async Task CreateProductAsync_ValidProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 19.99m,
            Colour = "Red"
        };
        
        // Act
        var product = await CreateProductUsingService(dto);
        
        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(dto.Name, product.Name);
        Assert.Equal(dto.Description, product.Description);
        Assert.Equal(dto.Price, product.Price);
        Assert.Equal(dto.Colour, product.Colour);
    }
    
    [Fact]
    public async Task CreateProductAsync_DatabaseError_ThrowsProductNotCreatedException()
    {
        // Arrange
        await using var context = new ProductDbContext(_options);
        var mockDbSet = new Mock<DbSet<Product>>();
        mockDbSet.As<IQueryable<Product>>();
        
        var mockProductContext = new Mock<ProductDbContext>(_options);
        mockProductContext.Setup(c => c.Products).Returns(mockDbSet.Object);
        mockProductContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0); 
        
        var service = new ProductService(mockProductContext.Object, _mockLogger.Object);
        
        var dto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 19.99m,
            Colour = "Red"
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotCreatedException>(() => 
            service.CreateProductAsync(dto));
        
        Assert.Contains(dto.Name, exception.Message);
    }
    
    [Fact]
    public async Task GetAllProductsAsync_ReturnsAllProducts()
    {
        // Arrange
        await SeedTestProducts();
        
        // Act
        await using var context = new ProductDbContext(_options);
        var service = new ProductService(context, _mockLogger.Object);
        var products = await service.GetAllProductsAsync();
        
        // Assert
        Assert.Equal(3, products.Count());
    }
    
    [Fact]
    public async Task GetProductsByColourAsync_ExistingColour_ReturnsFilteredProducts()
    {
        // Arrange
        await SeedTestProducts();
        
        // Act
        await using var context = new ProductDbContext(_options);
        var service = new ProductService(context, _mockLogger.Object);
        var products = await service.GetProductsByColourAsync("Red");
        
        // Assert
        Assert.Equal(2, products.Count());
        Assert.All(products, p => Assert.Equal("Red", p.Colour));
    }
    
    private async Task<Product> CreateProductUsingService(CreateProductDto dto)
    {
        await using var context = new ProductDbContext(_options);
        var service = new ProductService(context, _mockLogger.Object);
        return await service.CreateProductAsync(dto);
    }
    
    private async Task SeedTestProducts()
    {
        await using var context = new ProductDbContext(_options);
        var products = new List<Product>
        {
            new() { Name = "Product 1", Description = "Desc 1", Price = 10.99m, Colour = "Red", CreatedAt = DateTime.UtcNow },
            new() { Name = "Product 2", Description = "Desc 2", Price = 20.99m, Colour = "Blue", CreatedAt = DateTime.UtcNow },
            new() { Name = "Product 3", Description = "Desc 3", Price = 30.99m, Colour = "Red", CreatedAt = DateTime.UtcNow }
        };
        
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
} 
