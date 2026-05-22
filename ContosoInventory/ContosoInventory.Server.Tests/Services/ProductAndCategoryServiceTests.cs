using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ContosoInventory.Server.Data;
using ContosoInventory.Server.Models;
using ContosoInventory.Server.Repositories;
using ContosoInventory.Server.Services;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Tests.Services;

public class ProductAndCategoryServiceTests
{
    [Fact]
    public async Task DeleteCategoryAsync_CategoryHasProducts_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<InventoryContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new InventoryContext(options);

        var category = new Category
        {
            Name = "Peripherals",
            Description = "Test",
            DisplayOrder = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        context.Products.Add(new Product
        {
            Name = "Mouse",
            Sku = "MOUSE-001",
            Description = "Wireless mouse",
            Price = 12.50m,
            StockQuantity = 10,
            CategoryId = category.Id,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var logger = new Mock<ILogger<CategoryService>>();
        var repository = new CategoryRepository(context);
        var service = new CategoryService(repository, logger.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteCategoryAsync(category.Id));
    }

    [Fact]
    public async Task RestockProductAsync_QuantityCausesOverflow_ThrowsArgumentException()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        var logger = new Mock<ILogger<ProductService>>();

        repository
            .Setup(r => r.GetTrackedByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Laptop",
                Sku = "LAP-001",
                Description = "Work laptop",
                Price = 999.99m,
                StockQuantity = int.MaxValue,
                CategoryId = 2,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            });

        var service = new ProductService(repository.Object, logger.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.RestockProductAsync(1, new RestockProductDto { QuantityToAdd = 1 }));
    }

    [Fact]
    public async Task CreateProductAsync_SaveThrowsUniqueSkuViolation_ThrowsArgumentException()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        var logger = new Mock<ILogger<ProductService>>();

        repository.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        repository.Setup(r => r.CategoryExistsAsync(1)).ReturnsAsync(true);
        repository.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        repository
            .Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException(
                "Unable to save changes.",
                new Exception("UNIQUE constraint failed: Products.Sku")));

        var service = new ProductService(repository.Object, logger.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProductAsync(new CreateProductDto
        {
            Name = "Docking Station",
            Sku = "DOCK-001",
            Description = "USB-C dock",
            Price = 199.99m,
            StockQuantity = 3,
            CategoryId = 1
        }));
    }
}
