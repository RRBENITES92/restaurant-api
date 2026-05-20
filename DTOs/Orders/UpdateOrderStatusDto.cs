using RestaurantApi.Models;

namespace RestaurantApi.DTOs.Orders;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}