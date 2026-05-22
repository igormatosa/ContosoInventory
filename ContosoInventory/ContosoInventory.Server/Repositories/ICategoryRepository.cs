using ContosoInventory.Server.Models;

namespace ContosoInventory.Server.Repositories;

/// <summary>
/// Provides data access operations for categories.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Retrieves all categories ordered by display order.
    /// </summary>
    /// <returns>A list of categories.</returns>
    Task<List<Category>> GetAllAsync();

    /// <summary>
    /// Retrieves a category by identifier without tracking.
    /// </summary>
    /// <param name="id">The category identifier.</param>
    /// <returns>The category, or null if not found.</returns>
    Task<Category?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves a tracked category by identifier for updates.
    /// </summary>
    /// <param name="id">The category identifier.</param>
    /// <returns>The tracked category, or null if not found.</returns>
    Task<Category?> GetTrackedByIdAsync(int id);

    /// <summary>
    /// Checks whether a category name already exists.
    /// </summary>
    /// <param name="normalizedName">Lower-cased trimmed name value.</param>
    /// <param name="excludeCategoryId">Optional category identifier to exclude.</param>
    /// <returns>True if name exists; otherwise false.</returns>
    Task<bool> NameExistsAsync(string normalizedName, int? excludeCategoryId = null);

    /// <summary>
    /// Checks whether products exist for the category.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>True if products exist; otherwise false.</returns>
    Task<bool> HasProductsAsync(int categoryId);

    /// <summary>
    /// Adds a new category for persistence.
    /// </summary>
    /// <param name="category">The category entity.</param>
    Task AddAsync(Category category);

    /// <summary>
    /// Removes an existing category.
    /// </summary>
    /// <param name="category">The category entity.</param>
    void Remove(Category category);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    Task SaveChangesAsync();
}
