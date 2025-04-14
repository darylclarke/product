using FluentValidation.TestHelper;
using ProductsApi.DTOs;
using ProductsApi.Validators;

namespace ProductsApi.Tests;

public class ProductUnitTests
{
    private readonly CreateProductDtoValidator _validator = new();

    [Fact]
    public void ValidDto_PassesValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Valid Product",
            Description = "Valid Description",
            Price = 19.99m,
            Colour = "Red"
        };
        
        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void EmptyFields_FailsValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "",
            Description = "",
            Colour = ""
        };
        
        // Act
        var result = _validator.TestValidate(dto);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.Colour);
    }
    
    [Fact]
    public void ShortDescription_FailsValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Valid Product",
            Description = "Va",
            Price = 19.99m,
            Colour = "Red"
        };
        
        // Act
        var result = _validator.TestValidate(dto);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
    
    [Fact]
    public void NegativePrice_FailsValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Valid Product",
            Description = "Valid Description",
            Price = -1.99m, 
            Colour = "Red"
        };
        
        // Act
        var result = _validator.TestValidate(dto);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }
}
