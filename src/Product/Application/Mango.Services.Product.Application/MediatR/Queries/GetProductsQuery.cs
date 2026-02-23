using MediatR;
using Mango.Services.Product.Application.DTOs;
using Mango.Services.Product.Application.Interfaces;

namespace Mango.Services.Product.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve paginated list of products.
/// </summary>
public class GetProductsQuery : IRequest<PaginatedProductResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public GetProductsQuery()
    {
    }

    public GetProductsQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 ? Math.Min(pageSize, 100) : 10; // Cap at 100 items per page
    }
}

/// <summary>
/// Handler for GetProductsQuery.
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedProductResponse>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedProductResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _repository.GetAllAsync(request.PageNumber, request.PageSize);

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
