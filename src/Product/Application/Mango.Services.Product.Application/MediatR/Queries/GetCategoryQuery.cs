using MediatR;
using Mango.Services.Product.Application.DTOs;
using Mango.Services.Product.Application.Interfaces;

namespace Mango.Services.Product.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve a single category by ID.
/// </summary>
public class GetCategoryQuery : IRequest<CategoryDto?>
{
    public int Id { get; set; }

    public GetCategoryQuery(int id)
    {
        Id = id;
    }
}

/// <summary>
/// Handler for GetCategoryQuery.
/// </summary>
public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, CategoryDto?>
{
    private readonly IProductRepository _repository;

    public GetCategoryQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<CategoryDto?> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetCategoryByIdAsync(request.Id);

        if (category == null)
            return null;

        return MapCategoryDto(category);
    }

    private static CategoryDto MapCategoryDto(dynamic category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedBy = category.UpdatedBy
        };
    }
}
