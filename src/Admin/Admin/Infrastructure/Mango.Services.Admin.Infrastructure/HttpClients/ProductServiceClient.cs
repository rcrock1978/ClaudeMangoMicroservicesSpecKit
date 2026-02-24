using Serilog;

namespace Mango.Services.Admin.Infrastructure.HttpClients;

/// <summary>
/// HTTP client for communicating with the Product microservice.
/// </summary>
public interface IProductServiceClient
{
    /// <summary>Gets all products with pagination.</summary>
    Task<List<ProductDto>?> GetAllProductsAsync(int pageNumber = 1, int pageSize = 50);

    /// <summary>Gets a single product by ID.</summary>
    Task<ProductDto?> GetProductByIdAsync(int id);

    /// <summary>Gets total product count.</summary>
    Task<int> GetTotalProductCountAsync();

    /// <summary>Gets products by category.</summary>
    Task<List<ProductDto>?> GetProductsByCategoryAsync(int categoryId);

    /// <summary>Gets low stock products.</summary>
    Task<List<ProductDto>?> GetLowStockProductsAsync(int threshold = 10);

    /// <summary>Gets top selling products.</summary>
    Task<List<ProductDto>?> GetTopSellingProductsAsync(int count = 10);

    /// <summary>Gets all categories.</summary>
    Task<List<CategoryDto>?> GetAllCategoriesAsync();
}

public class ProductServiceClient : BaseServiceClient, IProductServiceClient
{
    public ProductServiceClient(HttpClient httpClient, ILogger logger)
        : base(httpClient, logger)
    {
    }

    public async Task<List<ProductDto>?> GetAllProductsAsync(int pageNumber = 1, int pageSize = 50)
    {
        _logger.Information("Fetching products: page {PageNumber}, size {PageSize}", pageNumber, pageSize);
        return await GetAsync<List<ProductDto>>($"/api/products?pageNumber={pageNumber}&pageSize={pageSize}");
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        _logger.Information("Fetching product {ProductId}", id);
        return await GetAsync<ProductDto>($"/api/products/{id}");
    }

    public async Task<int> GetTotalProductCountAsync()
    {
        _logger.Information("Fetching total product count");
        var result = await GetAsync<dynamic>("/api/products/count");
        return result?.count ?? 0;
    }

    public async Task<List<ProductDto>?> GetProductsByCategoryAsync(int categoryId)
    {
        _logger.Information("Fetching products for category {CategoryId}", categoryId);
        return await GetAsync<List<ProductDto>>($"/api/products/category/{categoryId}");
    }

    public async Task<List<ProductDto>?> GetLowStockProductsAsync(int threshold = 10)
    {
        _logger.Information("Fetching low stock products (threshold: {Threshold})", threshold);
        return await GetAsync<List<ProductDto>>($"/api/products/low-stock?threshold={threshold}");
    }

    public async Task<List<ProductDto>?> GetTopSellingProductsAsync(int count = 10)
    {
        _logger.Information("Fetching top {Count} selling products", count);
        return await GetAsync<List<ProductDto>>($"/api/products/top-selling?count={count}");
    }

    public async Task<List<CategoryDto>?> GetAllCategoriesAsync()
    {
        _logger.Information("Fetching all categories");
        return await GetAsync<List<CategoryDto>>("/api/products/categories");
    }
}

/// <summary>DTO for product data from Product service.</summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? Sales { get; set; } // Optional: sales count for metrics
}

/// <summary>DTO for category data from Product service.</summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
