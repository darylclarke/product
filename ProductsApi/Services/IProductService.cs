using ProductsApi.DTOs;
using ProductsApi.Models;

namespace ProductsApi.Services;

public interface IProductService
{
    Task<Product> CreateProductAsync(CreateProductDto productDto);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetProductsByColourAsync(string color);
}
