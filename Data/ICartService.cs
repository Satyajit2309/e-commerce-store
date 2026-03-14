using EcommerceStore.Models.Entities;

namespace EcommerceStore.Services.Interfaces;

public interface ICartService
{
    Task<List<CartItem>> GetCartItemsAsync(string userId);
    Task<int> GetCartCountAsync(string userId);
    Task<decimal> GetCartTotalAsync(string userId);
    Task<CartItem> AddToCartAsync(string userId, int productId, int quantity = 1);
    Task<CartItem?> UpdateQuantityAsync(string userId, int productId, int quantity);
    Task<bool> RemoveFromCartAsync(string userId, int productId);
    Task ClearCartAsync(string userId);
}
