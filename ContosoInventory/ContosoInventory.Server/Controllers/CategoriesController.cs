using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContosoInventory.Server.Services;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Controllers;

/// <summary>
/// Manages inventory category operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all categories ordered by display order.
    /// </summary>
    /// <returns>A list of all categories.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCategories()
    {
        _logger.LogInformation("Retrieving all categories.");

        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error retrieving categories.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving categories." });
        }
    }

    /// <summary>
    /// Returns a category by its unique identifier.
    /// </summary>
    /// <param name="id">The category identifier.</param>
    /// <returns>The category details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        _logger.LogInformation("Retrieving category with ID {CategoryId}.", id);

        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while retrieving category with ID {CategoryId}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Server error while retrieving category with ID {CategoryId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the category." });
        }
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="dto">The category creation data.</param>
    /// <returns>The created category.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        _logger.LogInformation("Creating new category '{CategoryName}'.", dto.Name);

        try
        {
            var category = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while creating category '{CategoryName}'.", dto.Name);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Server error while creating category '{CategoryName}'.", dto.Name);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the category." });
        }
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The category identifier.</param>
    /// <param name="dto">The updated category data.</param>
    /// <returns>The updated category.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] UpdateCategoryDto dto)
    {
        _logger.LogInformation("Updating category with ID {CategoryId}.", id);

        try
        {
            var category = await _categoryService.UpdateCategoryAsync(id, dto);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while updating category with ID {CategoryId}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Server error while updating category with ID {CategoryId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the category." });
        }
    }

    /// <summary>
    /// Deletes a category by its unique identifier.
    /// </summary>
    /// <param name="id">The category identifier.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCategory([FromRoute] int id)
    {
        _logger.LogInformation("Deleting category with ID {CategoryId}.", id);

        try
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while deleting category with ID {CategoryId}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Server error while deleting category with ID {CategoryId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the category." });
        }
    }

    /// <summary>
    /// Toggles the active status of a category.
    /// </summary>
    /// <param name="id">The category identifier.</param>
    /// <returns>The updated category.</returns>
    [HttpPost("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ToggleActive([FromRoute] int id)
    {
        _logger.LogInformation("Toggling active status for category with ID {CategoryId}.", id);

        try
        {
            var category = await _categoryService.ToggleActiveAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while toggling category with ID {CategoryId}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Server error while toggling category with ID {CategoryId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while toggling the category active status." });
        }
    }
}
