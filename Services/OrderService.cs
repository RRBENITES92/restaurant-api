using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs.Orders;
using RestaurantApi.DTOs.Orders.Responses;
using RestaurantApi.Models;
using RestaurantApi.Exceptions;

namespace RestaurantApi.Services;

public class OrderService
{
    private readonly RestaurantDbContext _db;
    private readonly ILogger<OrderService> _logger;

    public OrderService(RestaurantDbContext db, ILogger<OrderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, int userId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var order = new Order();

            decimal total = 0;

            var duplicatedProducts = dto.Items
                                    .GroupBy(i => i.ProductId)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key)
                                    .ToList();

            if (duplicatedProducts.Any())
            {
                throw new BadRequestException("Order contains duplicated products");
            }

            var productIds = dto.Items
                .Select(i => i.ProductId)
                .ToList();

            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in dto.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                if (product == null)
                {
                    throw new NotFoundException($"Product with id {item.ProductId} not found");
                }

                if (item.Quantity > product.Stock)
                {
                    throw new BadRequestException($"Not enough stock for {product.Name}");
                }

                var subtotal = product.Price * item.Quantity;

                var affectedRows = await _db.Products
                    .Where(p => p.Id == product.Id && p.Version == product.Version)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.Stock, p => p.Stock - item.Quantity)
                        .SetProperty(p => p.Version, p => p.Version + 1));

                if (affectedRows == 0)
                {
                    throw new BadRequestException("Stock was modified by another operation. Please try again.");
                }

                total += subtotal;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = subtotal
                };

                order.Items.Add(orderItem);
            }

            order.Total = total;
            order.UserId = userId;

            _db.Orders.Add(order);

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return new OrderResponseDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Total = order.Total,
                Status = order.Status,
                Items = order.Items.Select(i =>
                    new OrderItemResponseDto
                    {
                        ProductName = i.Product?.Name ?? "",
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Subtotal = i.Subtotal
                    }).ToList()
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new BadRequestException(
                "Stock was modified by another operation. Please try again.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrderResponseDto>> GetUserOrdersAsync(
    int userId)
    {
        var orders = await _db.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .Where(o => o.UserId == userId)
                    .ToListAsync();

        return orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
            CreatedAt = o.CreatedAt,
            Total = o.Total,
            Status = o.Status,
            Items = o.Items.Select(i =>
                new OrderItemResponseDto
                {
                    ProductName = i.Product!.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal
                }).ToList()

        }).ToList();
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(int orderId, OrderStatus status, int userId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }

        if (order.Status == OrderStatus.Completed)
        {
            throw new BadRequestException("Completed orders cannot be modified");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new BadRequestException("Cancelled orders cannot be modified");
        }

        var isValidTransition =
            (order.Status == OrderStatus.Pending &&
                (status == OrderStatus.Preparing || status == OrderStatus.Cancelled))
            ||
            (order.Status == OrderStatus.Preparing &&
                (status == OrderStatus.Completed || status == OrderStatus.Cancelled));

        if (!isValidTransition)
        {
            throw new BadRequestException($"Invalid status transition from {order.Status} to {status}");
        }

        var oldStatus = order.Status;

        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = order.Status,
            NewStatus = status,
            ChangedByUserId = userId
        };

        _db.OrderStatusHistories.Add(history);

        order.Status = status;

        await _db.SaveChangesAsync();

        return new OrderResponseDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            Total = order.Total,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                ProductName = i.Product!.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList()
        };
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ToListAsync();

        return orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
            CreatedAt = o.CreatedAt,
            Total = o.Total,
            Status = o.Status,
            Items = o.Items.Select(i => new OrderItemResponseDto
            {
                ProductName = i.Product!.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList()
        }).ToList();
    }

    public async Task<List<OrderStatusHistoryDto>> GetOrderHistoryAsync(int orderId)
    {
        var orderExists = await _db.Orders.AnyAsync(o => o.Id == orderId);

        if (!orderExists)
        {
            throw new NotFoundException("Order not found");
        }

        return await _db.OrderStatusHistories
            .Include(h => h.ChangedByUser)
            .Where(h => h.OrderId == orderId)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new OrderStatusHistoryDto
            {
                OldStatus = h.OldStatus.ToString(),
                NewStatus = h.NewStatus.ToString(),
                ChangedBy = h.ChangedByUser != null
                    ? h.ChangedByUser.Username
                    : "Unknown",
                ChangedAt = h.ChangedAt
            })
            .ToListAsync();
    }

}