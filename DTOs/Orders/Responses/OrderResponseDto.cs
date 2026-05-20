using RestaurantApi.Models;

namespace RestaurantApi.DTOs.Orders.Responses;

public class OrderResponseDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal Total { get; set; }

    public List<OrderItemResponseDto> Items { get; set; }
        = new();

    public OrderStatus Status { get; set; }
}