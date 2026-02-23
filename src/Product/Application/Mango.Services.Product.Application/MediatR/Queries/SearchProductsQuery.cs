using MediatR;
using Mango.Services.Product.Application.DTOs;
using Mango.Services.Product.Application.Interfaces;

namespace Mango.Services.Product.Application.MediatR.Queries;

/// <summary>
/// Query to search products by name or description.
/// </summary>
public class SearchProductsQuery : IRequest<PaginatedProductResponse>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public SearchProductsQuery()
    {
    }

    public SearchProductsQuery(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        SearchTerm = searchTerm ?? string.Empty;
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 ? Math.Min(pageSize, 100) : 10; // Cap at 100 items per page
    }
}

/// <summary>
/// Handler for SearchProductsQuery.
/// </summary>
public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, PaginatedProductResponse>
{
    private readonly IProductRepository _repository;

    public SearchProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedProductResponse> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _repository.SearchAsync(request.SearchTerm, request.PageNumber, request.PageSize);

        var items = products.Select(MapProductDto).ToList();

        return new PaginatedProductResponse
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static ProductDto MapProductDto(dynamic product)
    {
        return new ProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            IsAvailable = product.IsAvailable,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CreatedBy = product.CreatedBy,
            UpdatedBy = product.UpdatedBy
        };
    }
}
