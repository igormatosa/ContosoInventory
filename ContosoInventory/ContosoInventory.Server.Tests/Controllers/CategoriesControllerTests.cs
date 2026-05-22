using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ContosoInventory.Server.Controllers;
using ContosoInventory.Server.Services;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Tests.Controllers;

public class CategoriesControllerTests
{
    [Fact]
    public async Task GetCategoryById_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<ICategoryService>();
        var logger = new Mock<ILogger<CategoriesController>>();

        service
            .Setup(s => s.GetCategoryByIdAsync(0))
            .ThrowsAsync(new ArgumentException("Category ID must be greater than zero.", "id"));

        var controller = new CategoriesController(service.Object, logger.Object);

        // Act
        var result = await controller.GetCategoryById(0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateCategory_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<ICategoryService>();
        var logger = new Mock<ILogger<CategoriesController>>();

        var dto = new UpdateCategoryDto
        {
            Name = "Hardware",
            Description = "Hardware category",
            DisplayOrder = 1,
            IsActive = true
        };

        service
            .Setup(s => s.UpdateCategoryAsync(0, dto))
            .ThrowsAsync(new ArgumentException("Category ID must be greater than zero.", "id"));

        var controller = new CategoriesController(service.Object, logger.Object);

        // Act
        var result = await controller.UpdateCategory(0, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetAllCategories_ServiceThrowsInvalidOperation_ReturnsInternalServerError()
    {
        // Arrange
        var service = new Mock<ICategoryService>();
        var logger = new Mock<ILogger<CategoriesController>>();

        service
            .Setup(s => s.GetAllCategoriesAsync())
            .ThrowsAsync(new InvalidOperationException("Failed to retrieve categories."));

        var controller = new CategoriesController(service.Object, logger.Object);

        // Act
        var result = await controller.GetAllCategories();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<ICategoryService>();
        var logger = new Mock<ILogger<CategoriesController>>();

        service
            .Setup(s => s.DeleteCategoryAsync(0))
            .ThrowsAsync(new ArgumentException("Category ID must be greater than zero.", "id"));

        var controller = new CategoriesController(service.Object, logger.Object);

        // Act
        var result = await controller.DeleteCategory(0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteCategory_CategoryHasProducts_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<ICategoryService>();
        var logger = new Mock<ILogger<CategoriesController>>();

        service
            .Setup(s => s.DeleteCategoryAsync(1))
            .ThrowsAsync(new ArgumentException("Cannot delete category because it has associated products.", "id"));

        var controller = new CategoriesController(service.Object, logger.Object);

        // Act
        var result = await controller.DeleteCategory(1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ToggleActive_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<ICategoryService>();
        var logger = new Mock<ILogger<CategoriesController>>();

        service
            .Setup(s => s.ToggleActiveAsync(0))
            .ThrowsAsync(new ArgumentException("Category ID must be greater than zero.", "id"));

        var controller = new CategoriesController(service.Object, logger.Object);

        // Act
        var result = await controller.ToggleActive(0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ToggleActive_ServiceThrowsInvalidOperation_ReturnsInternalServerError()
    {
        // Arrange
        var service = new Mock<ICategoryService>();
        var logger = new Mock<ILogger<CategoriesController>>();

        service
            .Setup(s => s.ToggleActiveAsync(1))
            .ThrowsAsync(new InvalidOperationException("Failed to update category active status."));

        var controller = new CategoriesController(service.Object, logger.Object);

        // Act
        var result = await controller.ToggleActive(1);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
