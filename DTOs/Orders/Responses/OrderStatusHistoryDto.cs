namespace RestaurantApi.DTOs.Orders.Responses;

public class OrderStatusHistoryDto
{
    public string OldStatus { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }
}