using MediatR;
using Mango.Services.Product.Application.DTOs;
using Mango.Services.Product.Application.Interfaces;

namespace Mango.Services.Product.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve all active categories.
/// </summary>
public class GetCategoriesQuery : IRequest<List<CategoryDto>>
{
}

/// <summary>
/// Handler for GetCategoriesQuery.
/// </summary>
public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IProductRepository _repository;

    public GetCategoriesQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _repository.GetCategoriesAsync();

        return categories.Select(MapCategoryDto).ToList();
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
