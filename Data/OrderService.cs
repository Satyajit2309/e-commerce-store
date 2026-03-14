using EcommerceStore.Constants;
using EcommerceStore.Data;
using EcommerceStore.Models.Entities;
using EcommerceStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;

    public OrderService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Order> PlaceOrderAsync(string userId, string fullName, string address, string phone)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var cartItems = await _db.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Your cart is empty.");

            // Validate stock
            foreach (var item in cartItems)
            {
                if (item.Product is null || !item.Product.IsActive)
                    throw new InvalidOperationException($"Product \"{item.Product?.Name ?? "Unknown"}\" is no longer available.");

                if (item.Quantity > item.Product.Stock)
                    throw new InvalidOperationException($"Only {item.Product.Stock} units of \"{item.Product.Name}\" are available, but you have {item.Quantity} in your cart.");
            }

            var order = new Order
            {
                UserId = userId,
                FullName = fullName,
                ShippingAddress = address,
                PhoneNumber = phone,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatuses.Pending,
                TotalAmount = cartItems.Sum(ci => ci.Product!.Price * ci.Quantity)
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Create order items and reduce stock
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product!.Price,
                    LineTotal = item.Product.Price * item.Quantity
                };

                _db.OrderItems.Add(orderItem);
                item.Product.Stock -= item.Quantity;
            }

            // Clear cart
            _db.CartItems.RemoveRange(cartItems);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return order;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Order>> GetOrdersForUserAsync(string userId)
    {
        return await _db.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetAllOrdersAsync(string? statusFilter = null)
    {
        var query = _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(o => o.Status == statusFilter);

        return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
    }

    public async Task<Order?> UpdateOrderStatusAsync(int orderId, string newStatus)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order is null) return null;

        if (!OrderStatuses.IsValidTransition(order.Status, newStatus))
            throw new InvalidOperationException($"Cannot change status from \"{order.Status}\" to \"{newStatus}\".");

        order.Status = newStatus;
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<int> GetOrderCountAsync()
        => await _db.Orders.CountAsync();

    public async Task<decimal> GetTotalRevenueAsync()
        => await _db.Orders
            .Where(o => o.Status != OrderStatuses.Cancelled)
            .SumAsync(o => o.TotalAmount);
}
