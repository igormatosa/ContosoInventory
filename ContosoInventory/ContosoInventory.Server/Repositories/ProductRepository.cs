using Microsoft.EntityFrameworkCore;
using ContosoInventory.Server.Data;
using ContosoInventory.Server.Models;

namespace ContosoInventory.Server.Repositories;

/// <summary>
/// Implements data access operations for products.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly InventoryContext _context;

    public ProductRepository(InventoryContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<Product>> GetAllAsync(int? categoryId = null)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<Product?> GetTrackedByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<bool> SkuExistsAsync(string normalizedSku, int? excludeProductId = null)
    {
        return await _context.Products.AnyAsync(p =>
            EF.Functions.Collate(p.Sku, "NOCASE") == normalizedSku &&
            (!excludeProductId.HasValue || p.Id != excludeProductId.Value));
    }

    /// <inheritdoc />
    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories.AnyAsync(c => c.Id == categoryId);
    }

    /// <inheritdoc />
    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    /// <inheritdoc />
    public void Remove(Product product)
    {
        _context.Products.Remove(product);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
