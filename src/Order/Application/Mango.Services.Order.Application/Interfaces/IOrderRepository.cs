using OrderEntity = Mango.Services.Order.Domain.Entities.Order;
using OrderStatus = Mango.Services.Order.Domain.Entities.OrderStatus;

namespace Mango.Services.Order.Application.Interfaces;

/// <summary>
/// Repository interface for Order persistence operations.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Get an order by ID with related items.
    /// </summary>
    Task<OrderEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Get an order by unique order number.
    /// </summary>
    Task<OrderEntity?> GetByOrderNumberAsync(string orderNumber);

    /// <summary>
    /// Get orders by user ID with pagination.
    /// </summary>
    Task<(List<OrderEntity>, int)> GetByUserIdAsync(string userId, int pageNumber, int pageSize);

    /// <summary>
    /// Get orders by status with pagination.
    /// </summary>
    Task<(List<OrderEntity>, int)> GetByStatusAsync(OrderStatus status, int pageNumber, int pageSize);

    /// <summary>
    /// Get all orders with pagination.
    /// </summary>
    Task<(List<OrderEntity>, int)> GetAllAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Add a new order to the database.
    /// </summary>
    Task AddAsync(OrderEntity order);

    /// <summary>
    /// Update an existing order.
    /// </summary>
    Task UpdateAsync(OrderEntity order);

    /// <summary>
    /// Soft delete an order by ID.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
