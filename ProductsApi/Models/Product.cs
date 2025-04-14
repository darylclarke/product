namespace ProductsApi.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Colour { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
