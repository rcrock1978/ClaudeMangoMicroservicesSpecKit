using Microsoft.EntityFrameworkCore;
using Mango.Services.Product.Domain.Entities;
using Mango.Services.Product.Application.Interfaces;
using Mango.Services.Product.Infrastructure.Data;

namespace Mango.Services.Product.Infrastructure.Repositories;

/// <summary>
/// Product repository implementation with EF Core.
/// Handles all data access operations for products and categories.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(List<Product>, int)> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Products.AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<Product>, int)> SearchAsync(string query, int pageNumber, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync(pageNumber, pageSize);
        }

        var searchQuery = query.ToLower();
        var products = _context.Products.Where(p =>
            p.Name.ToLower().Contains(searchQuery) ||
            (p.Description != null && p.Description.ToLower().Contains(searchQuery)));

        var totalCount = await products.CountAsync();

        var items = await products
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<Product>, int)> GetByCategoryAsync(int categoryId, int pageNumber, int pageSize)
    {
        var query = _context.Products
            .Where(p => p.CategoryId == categoryId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.Products.AddAsync(product);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        var existing = await _context.Products.FindAsync(product.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Product with ID {product.Id} not found");
        }

        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {id} not found");
        }

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await SaveChangesAsync();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddCategoryAsync(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.Categories.AddAsync(category);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
