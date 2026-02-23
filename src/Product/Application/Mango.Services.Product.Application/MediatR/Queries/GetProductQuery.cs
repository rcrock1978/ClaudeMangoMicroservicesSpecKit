using MediatR;
using Mango.Services.Product.Application.DTOs;
using Mango.Services.Product.Application.Interfaces;

namespace Mango.Services.Product.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve a single product by ID.
/// </summary>
public class GetProductQuery : IRequest<ProductDto?>
{
    public int Id { get; set; }

    public GetProductQuery(int id)
    {
        Id = id;
    }
}

/// <summary>
/// Handler for GetProductQuery.
/// </summary>
public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto?>
{
    private readonly IProductRepository _repository;

    public GetProductQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);

        if (product == null)
            return null;

        return MapProductDto(product);
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
