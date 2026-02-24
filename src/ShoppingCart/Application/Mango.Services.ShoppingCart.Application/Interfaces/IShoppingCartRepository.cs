using ShoppingCartEntity = Mango.Services.ShoppingCart.Domain.Entities.ShoppingCart;

namespace Mango.Services.ShoppingCart.Application.Interfaces;

/// <summary>
/// Repository interface for Shopping Cart data access.
/// Implements repository pattern with async operations.
/// </summary>
public interface IShoppingCartRepository
{
    /// <summary>
    /// Get cart by ID.
    /// </summary>
    Task<ShoppingCartEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Get cart by user ID.
    /// </summary>
    Task<ShoppingCartEntity?> GetByUserIdAsync(string userId);

    /// <summary>
    /// Add new shopping cart.
    /// </summary>
    Task AddAsync(ShoppingCartEntity cart);

    /// <summary>
    /// Update existing shopping cart.
    /// </summary>
    Task UpdateAsync(ShoppingCartEntity cart);

    /// <summary>
    /// Delete shopping cart by ID.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Delete all cart items for a user (clear cart).
    /// </summary>
    Task ClearCartAsync(string userId);

    /// <summary>
    /// Save changes to database.
    /// </summary>
    Task SaveChangesAsync();
}
