using EcommerceStore.Data;
using EcommerceStore.Models.Entities;
using EcommerceStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Services.Implementations;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;

    public ProductService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetProductsAsync(string? search = null, int? categoryId = null, string? sortBy = null, bool includeInactive = false)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(lower) ||
                (p.Description != null && p.Description.ToLower().Contains(lower)));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        query = sortBy switch
        {
            "price_asc"  => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_desc"  => query.OrderByDescending(p => p.Name),
            _            => query.OrderBy(p => p.Name)
        };

        return await query.ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
        => await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> GetProductCountAsync()
        => await _db.Products.CountAsync();

    public async Task<Product> CreateProductAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateProductAsync(Product product)
    {
        var existing = await _db.Products.FindAsync(product.Id);
        if (existing is null) return null;

        existing.Name        = product.Name;
        existing.Description = product.Description;
        existing.Price       = product.Price;
        existing.Stock       = product.Stock;
        existing.ImageUrl    = product.ImageUrl;
        existing.IsActive    = product.IsActive;
        existing.CategoryId  = product.CategoryId;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return false;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return true;
    }
}
