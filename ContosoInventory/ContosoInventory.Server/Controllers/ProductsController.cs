using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContosoInventory.Server.Services;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Controllers;

/// <summary>
/// Manages inventory product operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all products with optional category filtering.
    /// </summary>
    /// <param name="categoryId">Optional category identifier for filtering.</param>
    /// <returns>A list of products.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllProducts([FromQuery] int? categoryId = null)
    {
        _logger.LogInformation("Retrieving products for CategoryId {CategoryId}.", categoryId);

        try
        {
            var products = await _productService.GetAllProductsAsync(categoryId);
            return Ok(products);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while retrieving products.");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        _logger.LogInformation("Retrieving product with ID {ProductId}.", id);

        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while retrieving product with ID {ProductId}.", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="dto">The product creation data.</param>
    /// <returns>The created product.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        _logger.LogInformation("Creating new product with SKU {Sku}.", dto.Sku);

        try
        {
            var product = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while creating product with SKU {Sku}.", dto.Sku);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Server error while creating product with SKU {Sku}.", dto.Sku);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the product." });
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">The updated product data.</param>
    /// <returns>The updated product.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product with ID {ProductId}.", id);

        try
        {
            var product = await _productService.UpdateProductAsync(id, dto);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while updating product with ID {ProductId}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Server error while updating product with ID {ProductId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the product." });
        }
    }

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct([FromRoute] int id)
    {
        _logger.LogInformation("Deleting product with ID {ProductId}.", id);

        try
        {
            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while deleting product with ID {ProductId}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Server error while deleting product with ID {ProductId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the product." });
        }
    }

    /// <summary>
    /// Increases stock quantity for a specific product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">The restock data.</param>
    /// <returns>The updated product.</returns>
    [HttpPost("{id}/restock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestockProduct([FromRoute] int id, [FromBody] RestockProductDto dto)
    {
        _logger.LogInformation(
            "Restocking product with ID {ProductId} by quantity {QuantityToAdd}.",
            id,
            dto.QuantityToAdd);

        try
        {
            var product = await _productService.RestockProductAsync(id, dto);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while restocking product with ID {ProductId}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Server error while restocking product with ID {ProductId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while restocking the product." });
        }
    }
}
