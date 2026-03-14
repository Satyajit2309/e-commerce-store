using EcommerceStore.Data;
using EcommerceStore.Models.Entities;
using EcommerceStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Services.Implementations;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _db;

    public CartService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CartItem>> GetCartItemsAsync(string userId)
    {
        return await _db.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p!.Category)
            .Where(ci => ci.UserId == userId)
            .OrderByDescending(ci => ci.AddedAt)
            .ToListAsync();
    }

    public async Task<int> GetCartCountAsync(string userId)
    {
        return await _db.CartItems
            .Where(ci => ci.UserId == userId)
            .SumAsync(ci => ci.Quantity);
    }

    public async Task<decimal> GetCartTotalAsync(string userId)
    {
        return await _db.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .SumAsync(ci => ci.Product!.Price * ci.Quantity);
    }

    public async Task<CartItem> AddToCartAsync(string userId, int productId, int quantity = 1)
    {
        if (quantity < 1)
            throw new ArgumentException("Quantity must be at least 1.", nameof(quantity));

        var product = await _db.Products.FindAsync(productId)
            ?? throw new InvalidOperationException("Product not found.");

        if (!product.IsActive)
            throw new InvalidOperationException("Product is not available.");

        var existing = await _db.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (existing is not null)
        {
            var newQty = existing.Quantity + quantity;
            if (newQty > product.Stock)
                throw new InvalidOperationException($"Only {product.Stock} units available. You already have {existing.Quantity} in your cart.");

            existing.Quantity = newQty;
            await _db.SaveChangesAsync();
            return existing;
        }

        if (quantity > product.Stock)
            throw new InvalidOperationException($"Only {product.Stock} units available.");

        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = productId,
            Quantity = quantity,
            AddedAt = DateTime.UtcNow
        };

        _db.CartItems.Add(cartItem);
        await _db.SaveChangesAsync();
        return cartItem;
    }

    public async Task<CartItem?> UpdateQuantityAsync(string userId, int productId, int quantity)
    {
        var cartItem = await _db.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (cartItem is null) return null;

        if (quantity <= 0)
        {
            _db.CartItems.Remove(cartItem);
            await _db.SaveChangesAsync();
            return null;
        }

        if (quantity > cartItem.Product!.Stock)
            throw new InvalidOperationException($"Only {cartItem.Product.Stock} units available.");

        cartItem.Quantity = quantity;
        await _db.SaveChangesAsync();
        return cartItem;
    }

    public async Task<bool> RemoveFromCartAsync(string userId, int productId)
    {
        var cartItem = await _db.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (cartItem is null) return false;

        _db.CartItems.Remove(cartItem);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task ClearCartAsync(string userId)
    {
        var items = await _db.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}
