namespace RestaurantApi.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; } = true;

    //public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public int Version { get; set; } = 1;
    
}