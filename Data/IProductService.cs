using EcommerceStore.Models.Entities;

namespace EcommerceStore.Services.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetProductsAsync(string? search = null, int? categoryId = null, string? sortBy = null, bool includeInactive = false);
    Task<Product?> GetProductByIdAsync(int id);
    Task<int> GetProductCountAsync();
    Task<Product> CreateProductAsync(Product product);
    Task<Product?> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int id);
    Task<int> GetLowStockProductCountAsync(int threshold = 5);
}
