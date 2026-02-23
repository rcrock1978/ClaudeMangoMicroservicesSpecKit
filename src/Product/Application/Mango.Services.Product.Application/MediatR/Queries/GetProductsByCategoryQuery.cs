using MediatR;
using Mango.Services.Product.Application.DTOs;
using Mango.Services.Product.Application.Interfaces;

namespace Mango.Services.Product.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve products by category with pagination.
/// </summary>
public class GetProductsByCategoryQuery : IRequest<PaginatedProductResponse>
{
    public int CategoryId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public GetProductsByCategoryQuery()
    {
    }

    public GetProductsByCategoryQuery(int categoryId, int pageNumber = 1, int pageSize = 10)
    {
        CategoryId = categoryId;
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 ? Math.Min(pageSize, 100) : 10; // Cap at 100 items per page
    }
}

/// <summary>
/// Handler for GetProductsByCategoryQuery.
/// </summary>
public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, PaginatedProductResponse>
{
    private readonly IProductRepository _repository;

    public GetProductsByCategoryQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedProductResponse> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _repository.GetByCategoryAsync(request.CategoryId, request.PageNumber, request.PageSize);

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
