using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RestaurantApi.DTOs.Orders;
using RestaurantApi.Models;
using RestaurantApi.Services;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
    CreateOrderDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var order = await _orderService
            .CreateOrderAsync(dto, userId);

        return Ok(order);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

        var orders = await _orderService
            .GetUserOrdersAsync(userId);

        return Ok(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

        var order = await _orderService.UpdateOrderStatusAsync(id, dto.Status, userId);

        return Ok(order);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();

        return Ok(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetOrderHistory(int id)
    {
        var history = await _orderService.GetOrderHistoryAsync(id);

        return Ok(history);
    }

}