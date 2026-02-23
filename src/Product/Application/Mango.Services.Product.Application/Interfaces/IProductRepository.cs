using ProductEntity = Mango.Services.Product.Domain.Entities.Product;
using CategoryEntity = Mango.Services.Product.Domain.Entities.Category;

namespace Mango.Services.Product.Application.Interfaces;

/// <summary>
/// Repository interface for Product data access.
/// Implements repository pattern with async operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Get product by ID (includes related category).
    /// </summary>
    Task<ProductEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Get all active products with pagination.
    /// </summary>
    Task<(List<ProductEntity> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Search products by name/description with pagination.
    /// </summary>
    Task<(List<ProductEntity> Items, int TotalCount)> SearchAsync(
        string query,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// Get products by category with pagination.
    /// </summary>
    Task<(List<ProductEntity> Items, int TotalCount)> GetByCategoryAsync(
        int categoryId,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// Add new product.
    /// </summary>
    Task AddAsync(ProductEntity product);

    /// <summary>
    /// Update existing product.
    /// </summary>
    Task UpdateAsync(ProductEntity product);

    /// <summary>
    /// Soft delete product.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Get all categories.
    /// </summary>
    Task<List<CategoryEntity>> GetCategoriesAsync();

    /// <summary>
    /// Get category by ID.
    /// </summary>
    Task<CategoryEntity?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// Add new category.
    /// </summary>
    Task AddCategoryAsync(CategoryEntity category);

    /// <summary>
    /// Save changes to database.
    /// </summary>
    Task SaveChangesAsync();
}
