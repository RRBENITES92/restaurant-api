namespace RestaurantApi.DTOs.Orders.Responses;

public class OrderItemResponseDto
{
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }
}