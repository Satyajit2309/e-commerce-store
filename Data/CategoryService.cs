using EcommerceStore.Data;
using EcommerceStore.Models.Entities;
using EcommerceStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _db;

    public CategoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetCategoriesAsync()
        => await _db.Categories.OrderBy(c => c.Name).ToListAsync();

    public async Task<Category?> GetCategoryByIdAsync(int id)
        => await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(Category category)
    {
        var existing = await _db.Categories.FindAsync(category.Id);
        if (existing is null) return null;

        existing.Name        = category.Name;
        existing.Description = category.Description;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return false;
        if (category.Products.Any()) return false; // prevent deleting with products

        try
        {
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}
