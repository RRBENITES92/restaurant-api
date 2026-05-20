namespace RestaurantApi.DTOs.Orders;

public class CreateOrderDto
{
    public List<CreateOrderItemDto> Items { get; set; } = new();
}