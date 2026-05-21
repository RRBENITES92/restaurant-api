using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs;

namespace RestaurantApi.Validators;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    private readonly RestaurantDbContext _db;

    public ProductCreateDtoValidator(RestaurantDbContext db)
    {
        _db = db;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(50)
            .WithMessage("Product name cannot exceed 50 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Product price must be greater than zero");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId must be greater than zero");
    }

}