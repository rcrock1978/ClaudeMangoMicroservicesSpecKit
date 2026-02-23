using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.Product.Application.MediatR.Queries;
using Mango.Services.Product.Application.DTOs;

namespace Mango.Services.Product.API.Controllers;

/// <summary>
/// API controller for product operations.
/// Provides endpoints for browsing and searching products.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a single product by ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details or 404 if not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var query = new GetProductQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get paginated list of all products.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedProductResponse>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetProductsQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Search products by name or description.
    /// </summary>
    /// <param name="searchTerm">Search term to find in product name or description</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <returns>Paginated list of matching products</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedProductResponse>> SearchProducts(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term cannot be empty");

        var query = new SearchProductsQuery(searchTerm, pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get products by category with pagination.
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <returns>Paginated list of products in category</returns>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(typeof(PaginatedProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedProductResponse>> GetProductsByCategory(
        int categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetProductsByCategoryQuery(categoryId, pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
