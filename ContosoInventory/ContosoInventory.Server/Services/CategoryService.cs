using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using ContosoInventory.Server.Models;
using ContosoInventory.Server.Repositories;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Provides operations for managing inventory categories.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(MapToDto).ToList();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error retrieving all categories.");
            throw new InvalidOperationException("Failed to retrieve categories.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error retrieving all categories.");
            throw new InvalidOperationException("Failed to retrieve categories.", ex);
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "Database error retrieving all categories.");
            throw new InvalidOperationException("Failed to retrieve categories.", ex);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while retrieving all categories.");
            throw new InvalidOperationException("Failed to retrieve categories.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(id));
        }

        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            return category == null ? null : MapToDto(category);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error retrieving category with ID {CategoryId}.", id);
            throw new InvalidOperationException("Failed to retrieve category.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error retrieving category with ID {CategoryId}.", id);
            throw new InvalidOperationException("Failed to retrieve category.", ex);
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "Database error retrieving category with ID {CategoryId}.", id);
            throw new InvalidOperationException("Failed to retrieve category.", ex);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while retrieving category with ID {CategoryId}.", id);
            throw new InvalidOperationException("Failed to retrieve category.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentException("Category data is required.", nameof(dto));
        }

        ValidateCategoryInput(dto.Name, dto.Description, dto.DisplayOrder);

        try
        {
            var normalizedName = dto.Name.Trim().ToLowerInvariant();

            // Check for duplicate name (case-insensitive)
            var exists = await _categoryRepository.NameExistsAsync(normalizedName);

            if (exists)
            {
                throw new ArgumentException($"A category with the name '{dto.Name}' already exists.", nameof(dto.Name));
            }

            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category created: {CategoryName} (ID: {CategoryId}).", category.Name, category.Id);

            return MapToDto(category);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error creating category '{CategoryName}'.", dto.Name);
            throw new InvalidOperationException("Failed to save category changes.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CategoryResponseDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(id));
        }

        if (dto == null)
        {
            throw new ArgumentException("Category data is required.", nameof(dto));
        }

        ValidateCategoryInput(dto.Name, dto.Description, dto.DisplayOrder);

        try
        {
            var category = await _categoryRepository.GetTrackedByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            var normalizedName = dto.Name.Trim().ToLowerInvariant();

            // Check for duplicate name (case-insensitive), excluding current category
            var exists = await _categoryRepository.NameExistsAsync(normalizedName, id);

            if (exists)
            {
                throw new ArgumentException($"A category with the name '{dto.Name}' already exists.", nameof(dto.Name));
            }

            category.Name = dto.Name.Trim();
            category.Description = dto.Description.Trim();
            category.DisplayOrder = dto.DisplayOrder;
            category.IsActive = dto.IsActive;
            category.LastModifiedDate = DateTime.UtcNow;

            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category updated: {CategoryName} (ID: {CategoryId}).", category.Name, category.Id);

            return MapToDto(category);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error updating category with ID {CategoryId}.", id);
            throw new InvalidOperationException("Failed to save category changes.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteCategoryAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(id));
        }

        try
        {
            var category = await _categoryRepository.GetTrackedByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            var hasProducts = await _categoryRepository.HasProductsAsync(id);
            if (hasProducts)
            {
                throw new ArgumentException("Cannot delete category because it has associated products.", nameof(id));
            }

            _categoryRepository.Remove(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category deleted: {CategoryName} (ID: {CategoryId}).", category.Name, category.Id);

            return true;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error deleting category with ID {CategoryId}.", id);
            throw new InvalidOperationException("Cannot delete category because it has associated products.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CategoryResponseDto?> ToggleActiveAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(id));
        }

        try
        {
            var category = await _categoryRepository.GetTrackedByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            category.IsActive = !category.IsActive;
            category.LastModifiedDate = DateTime.UtcNow;

            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category toggled: {CategoryName} (ID: {CategoryId}) is now {Status}.",
                category.Name, category.Id, category.IsActive ? "active" : "inactive");

            return MapToDto(category);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error toggling active status for category with ID {CategoryId}.", id);
            throw new InvalidOperationException("Failed to update category active status.", ex);
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "Database error toggling active status for category with ID {CategoryId}.", id);
            throw new InvalidOperationException("Failed to update category active status.", ex);
        }
    }

    private static CategoryResponseDto MapToDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedDate = category.CreatedDate,
            LastModifiedDate = category.LastModifiedDate
        };
    }

    private static void ValidateCategoryInput(string name, string description, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Category description is required.", nameof(description));
        }

        if (displayOrder < 1 || displayOrder > 1000)
        {
            throw new ArgumentException("Display order must be between 1 and 1000.", nameof(displayOrder));
        }
    }
}
