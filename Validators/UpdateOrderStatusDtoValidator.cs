using FluentValidation;
using RestaurantApi.DTOs.Orders;
using RestaurantApi.Models;

namespace RestaurantApi.Validators;

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid order status");
    }
}