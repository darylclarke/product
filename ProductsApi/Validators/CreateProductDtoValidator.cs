using FluentValidation;
using ProductsApi.DTOs;

namespace ProductsApi.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(5).WithMessage("Description must be at least 5 characters long")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("Price is required")
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Colour)
            .NotEmpty().WithMessage("Colour is required")
            .MaximumLength(50).WithMessage("Colour cannot exceed 50 characters");
    }
} 