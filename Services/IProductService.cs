using RestaurantApi.DTOs;

namespace RestaurantApi.Services;

public interface IProductService
{
    Task<PagedResultDto<ProductResponseDto>> GetProductsAsync(int page, int pageSizes, string? search, string? sortBy);

    Task<ProductResponseDto?> GetProductByIdAsync(int id);

    Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto);

    Task<ProductResponseDto?> UpdateProductAsync(int id, ProductCreateDto dto);

    Task<bool> DeleteProductAsync(int id);

    Task<bool> DeactivateProductAsync(int id);
}