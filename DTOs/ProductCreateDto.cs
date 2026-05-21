using System.ComponentModel.DataAnnotations;

namespace RestaurantApi.DTOs;

public class ProductCreateDto
{
    
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int? CategoryId { get; set; }
}