using System.ComponentModel.DataAnnotations;

namespace RestaurantApi.DTOs;

public class ProductCreateDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 10000)]
    public decimal Price { get; set; }

    public int? CategoryId { get; set; }
}