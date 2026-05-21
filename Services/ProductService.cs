using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs;
using RestaurantApi.Models;
using RestaurantApi.Exceptions;

namespace RestaurantApi.Services;

public class ProductService : IProductService
{
    private readonly RestaurantDbContext _db;

    public ProductService(RestaurantDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<ProductResponseDto>> GetProductsAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy)
    {
        var query = _db.Products.Include(p => p.Category).Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
        }

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "name":
                    query = query.OrderBy(p => p.Name);
                    break;

                case "price":
                    query = query.OrderBy(p => (double)p.Price);
                    break;
            }
        }

        var products = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CategoryName = p.Category != null ? p.Category.Name : null
        })
        .ToListAsync();

        return new PagedResultDto<ProductResponseDto>
        {
            Items = products,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductResponseDto> GetProductByIdAsync(int id)
    {
        var product = await _db.Products
        .Include(p => p.Category)
        .Where(p => p.IsActive)
        .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }

        return MapToResponseDto(product);
    }

    public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto)
    {
        await ValidateCategoryExistsAsync(dto.CategoryId);

        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            CategoryId = dto.CategoryId
        };

        _db.Products.Add(product);

        await _db.SaveChangesAsync();

        return MapToResponseDto(product);
    }

    public async Task<ProductResponseDto?> UpdateProductAsync(int id, ProductCreateDto dto)
    {
        var product = await _db.Products
        .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }

        await ValidateCategoryExistsAsync(dto.CategoryId);

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.CategoryId = dto.CategoryId;

        await _db.SaveChangesAsync();

        return MapToResponseDto(product);
    }

    public async Task DeactivateProductAsync(int id)
    {
        var product = await _db.Products
        .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }

        product.IsActive = false;

        await _db.SaveChangesAsync();
    }

    private static ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CategoryName = product.Category?.Name
        };
    }

    private async Task ValidateCategoryExistsAsync(int? categoryId)
    {
        if (!categoryId.HasValue)
        {
            return;
        }

        var categoryExists = await _db.Categories
            .AnyAsync(c => c.Id == categoryId.Value);

        if (!categoryExists)
        {
            throw new BadRequestException("Category does not exist");
        }
    }

}