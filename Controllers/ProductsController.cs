using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using RestaurantApi.Data;
using RestaurantApi.Models;
using RestaurantApi.DTOs;
using RestaurantApi.Services;
using RestaurantApi.Responses;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? sortBy = null)
    {
        _logger.LogInformation("Getting all products");

        var result = await _productService.GetProductsAsync(page, pageSize, search, sortBy);

        return Ok(new ApiResponse<PagedResultDto<ProductResponseDto>>
        {
            Success = true,
            Message = "Products retrieved successfully",
            Data = result
        });
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        return Ok(new ApiResponse<ProductResponseDto>
        {
            Success = true,
            Message = "Product found",
            Data = product
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
    {
        var product = await _productService.CreateProductAsync(dto);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductCreateDto dto)
    {
        var product = await _productService.UpdateProductAsync(id, dto);

        return Ok(new ApiResponse<ProductResponseDto>
        {
            Success = true,
            Message = "Product updated successfully",
            Data = product
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateProduct(int id)
    {
        await _productService.DeactivateProductAsync(id);

        return NoContent();
    }
}