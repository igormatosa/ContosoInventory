using Microsoft.EntityFrameworkCore;
using ContosoInventory.Server.Models;
using ContosoInventory.Server.Repositories;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Provides operations for managing inventory products.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ProductResponseDto>> GetAllProductsAsync(int? categoryId = null)
    {
        if (categoryId.HasValue && categoryId.Value <= 0)
        {
            throw new ArgumentException("CategoryId must be greater than zero.", nameof(categoryId));
        }

        _logger.LogInformation("Retrieving all products for CategoryId {CategoryId}.", categoryId);

        var products = await _productRepository.GetAllAsync(categoryId);

        return products.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Product ID must be greater than zero.", nameof(id));
        }

        _logger.LogInformation("Retrieving product with ID {ProductId}.", id);

        var product = await _productRepository.GetByIdAsync(id);

        return product == null ? null : MapToDto(product);
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto)
    {
        ValidateCreateDto(dto);

        try
        {
            await ValidateSkuUniquenessAsync(dto.Sku);
            await ValidateCategoryExistsAsync(dto.CategoryId);

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Sku = dto.Sku.Trim(),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product created: {Sku} (ID: {ProductId}).", product.Sku, product.Id);

            return MapToDto(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DbUpdateException ex) when (IsSkuUniqueConstraintViolation(ex))
        {
            _logger.LogWarning("SKU conflict detected while creating product with SKU {Sku}.", dto.Sku);
            throw new ArgumentException($"A product with SKU '{dto.Sku}' already exists.", nameof(dto));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error creating product with SKU {Sku}.", dto.Sku);
            throw new InvalidOperationException("Failed to save product changes.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Product ID must be greater than zero.", nameof(id));
        }

        ValidateUpdateDto(dto);

        try
        {
            var product = await _productRepository.GetTrackedByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            await ValidateSkuUniquenessAsync(dto.Sku, id);
            await ValidateCategoryExistsAsync(dto.CategoryId);

            product.Name = dto.Name.Trim();
            product.Sku = dto.Sku.Trim();
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            product.LastUpdatedDate = DateTime.UtcNow;

            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product updated: {Sku} (ID: {ProductId}).", product.Sku, product.Id);

            return MapToDto(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DbUpdateException ex) when (IsSkuUniqueConstraintViolation(ex))
        {
            _logger.LogWarning("SKU conflict detected while updating product with SKU {Sku}.", dto.Sku);
            throw new ArgumentException($"A product with SKU '{dto.Sku}' already exists.", nameof(dto));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}.", id);
            throw new InvalidOperationException("Failed to save product changes.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProductAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Product ID must be greater than zero.", nameof(id));
        }

        try
        {
            var product = await _productRepository.GetTrackedByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            _productRepository.Remove(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product deleted: {Sku} (ID: {ProductId}).", product.Sku, product.Id);

            return true;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}.", id);
            throw new InvalidOperationException("Failed to delete product.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> RestockProductAsync(int id, RestockProductDto dto)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Product ID must be greater than zero.", nameof(id));
        }

        ValidateRestockDto(dto);

        try
        {
            var product = await _productRepository.GetTrackedByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            if (product.StockQuantity > int.MaxValue - dto.QuantityToAdd)
            {
                throw new ArgumentException("Restock would exceed the maximum allowed stock quantity.", nameof(dto));
            }

            product.StockQuantity += dto.QuantityToAdd;
            product.LastUpdatedDate = DateTime.UtcNow;

            await _productRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Product restocked: {Sku} (ID: {ProductId}) quantity increased by {QuantityToAdd}.",
                product.Sku,
                product.Id,
                dto.QuantityToAdd);

            return MapToDto(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error restocking product with ID {ProductId}.", id);
            throw new InvalidOperationException("Failed to restock product.", ex);
        }
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CreatedDate = product.CreatedDate,
            LastUpdatedDate = product.LastUpdatedDate
        };
    }

    private static void ValidateCreateDto(CreateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Product name is required.", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Sku))
        {
            throw new ArgumentException("SKU is required.", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ArgumentException("Description is required.", nameof(dto));
        }

        if (dto.Price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.", nameof(dto));
        }

        if (dto.StockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(dto));
        }

        if (dto.CategoryId <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(dto));
        }
    }

    private static void ValidateUpdateDto(UpdateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Product name is required.", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Sku))
        {
            throw new ArgumentException("SKU is required.", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ArgumentException("Description is required.", nameof(dto));
        }

        if (dto.Price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.", nameof(dto));
        }

        if (dto.StockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(dto));
        }

        if (dto.CategoryId <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(dto));
        }
    }

    private static void ValidateRestockDto(RestockProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.QuantityToAdd <= 0)
        {
            throw new ArgumentException("Restock quantity must be greater than zero.", nameof(dto));
        }
    }

    private async Task ValidateSkuUniquenessAsync(string sku, int? excludeProductId = null)
    {
        var normalizedSku = sku.Trim().ToLowerInvariant();

        var exists = await _productRepository.SkuExistsAsync(normalizedSku, excludeProductId);

        if (exists)
        {
            throw new ArgumentException($"A product with SKU '{sku}' already exists.", nameof(sku));
        }
    }

    private async Task ValidateCategoryExistsAsync(int categoryId)
    {
        var categoryExists = await _productRepository.CategoryExistsAsync(categoryId);

        if (!categoryExists)
        {
            throw new ArgumentException($"Category with ID '{categoryId}' does not exist.", nameof(categoryId));
        }
    }

    private static bool IsSkuUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains("UNIQUE constraint failed: Products.Sku", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("IX_Products_Sku", StringComparison.OrdinalIgnoreCase);
    }
}
