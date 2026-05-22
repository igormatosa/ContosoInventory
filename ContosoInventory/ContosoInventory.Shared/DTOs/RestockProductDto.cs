namespace ContosoInventory.Shared.DTOs;

/// <summary>
/// Represents restock data for a product.
/// </summary>
public class RestockProductDto
{
    [Range(1, int.MaxValue)]
    public int QuantityToAdd { get; set; }
}
