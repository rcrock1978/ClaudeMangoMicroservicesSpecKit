using Microsoft.EntityFrameworkCore;
using OrderEntity = Mango.Services.Order.Domain.Entities.Order;
using OrderStatus = Mango.Services.Order.Domain.Entities.OrderStatus;
using Mango.Services.Order.Application.Interfaces;
using Mango.Services.Order.Infrastructure.Data;

namespace Mango.Services.Order.Infrastructure.Repositories;

/// <summary>
/// Order repository implementation with EF Core.
/// Handles all data access operations for orders.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<OrderEntity?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<OrderEntity?> GetByOrderNumberAsync(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return null;

        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<(List<OrderEntity>, int)> GetByUserIdAsync(string userId, int pageNumber, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return (new List<OrderEntity>(), 0);

        var query = _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<OrderEntity>, int)> GetByStatusAsync(OrderStatus status, int pageNumber, int pageSize)
    {
        var query = _context.Orders
            .Where(o => o.OrderStatus == status)
            .Include(o => o.Items);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<OrderEntity>, int)> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Orders.Include(o => o.Items);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(OrderEntity order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        foreach (var item in order.Items)
        {
            if (item.CreatedAt == DateTime.MinValue)
                item.CreatedAt = DateTime.UtcNow;
            if (item.UpdatedAt == DateTime.MinValue)
                item.UpdatedAt = DateTime.UtcNow;
        }

        await _context.Orders.AddAsync(order);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(OrderEntity order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        var existing = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        if (existing == null)
            throw new InvalidOperationException($"Order with ID {order.Id} not found");

        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            throw new InvalidOperationException($"Order with ID {id} not found");

        order.IsDeleted = true;
        order.DeletedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
