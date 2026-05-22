using ContosoInventory.Server.Models;

namespace ContosoInventory.Server.Repositories;

/// <summary>
/// Provides data access operations for products.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves all products with optional category filtering.
    /// </summary>
    /// <param name="categoryId">Optional category identifier.</param>
    /// <returns>A list of products.</returns>
    Task<List<Product>> GetAllAsync(int? categoryId = null);

    /// <summary>
    /// Retrieves a product by identifier without tracking.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product, or null if not found.</returns>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves a tracked product by identifier for updates.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The tracked product, or null if not found.</returns>
    Task<Product?> GetTrackedByIdAsync(int id);

    /// <summary>
    /// Checks whether a SKU already exists.
    /// </summary>
    /// <param name="normalizedSku">Lower-cased trimmed SKU value.</param>
    /// <param name="excludeProductId">Optional product identifier to exclude.</param>
    /// <returns>True if SKU exists; otherwise false.</returns>
    Task<bool> SkuExistsAsync(string normalizedSku, int? excludeProductId = null);

    /// <summary>
    /// Checks whether a category exists.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>True if category exists; otherwise false.</returns>
    Task<bool> CategoryExistsAsync(int categoryId);

    /// <summary>
    /// Adds a new product for persistence.
    /// </summary>
    /// <param name="product">The product entity.</param>
    Task AddAsync(Product product);

    /// <summary>
    /// Removes an existing product.
    /// </summary>
    /// <param name="product">The product entity.</param>
    void Remove(Product product);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    Task SaveChangesAsync();
}
