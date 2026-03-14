using EcommerceStore.Models.Entities;

namespace EcommerceStore.Services.Interfaces;

public interface IOrderService
{
    Task<Order> PlaceOrderAsync(string userId, string fullName, string address, string phone);
    Task<List<Order>> GetOrdersForUserAsync(string userId);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<List<Order>> GetAllOrdersAsync(string? statusFilter = null);
    Task<Order?> UpdateOrderStatusAsync(int orderId, string newStatus);
    Task<int> GetOrderCountAsync();
    Task<decimal> GetTotalRevenueAsync();
}
