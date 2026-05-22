using Microsoft.EntityFrameworkCore;
using ContosoInventory.Server.Data;
using ContosoInventory.Server.Models;

namespace ContosoInventory.Server.Repositories;

/// <summary>
/// Implements data access operations for categories.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly InventoryContext _context;

    public CategoryRepository(InventoryContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <inheritdoc />
    public async Task<Category?> GetTrackedByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<bool> NameExistsAsync(string normalizedName, int? excludeCategoryId = null)
    {
        return await _context.Categories.AnyAsync(c =>
            EF.Functions.Collate(c.Name, "NOCASE") == normalizedName &&
            (!excludeCategoryId.HasValue || c.Id != excludeCategoryId.Value));
    }

    /// <inheritdoc />
    public async Task<bool> HasProductsAsync(int categoryId)
    {
        return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
    }

    /// <inheritdoc />
    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    /// <inheritdoc />
    public void Remove(Category category)
    {
        _context.Categories.Remove(category);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
