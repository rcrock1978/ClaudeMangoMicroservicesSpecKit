using Microsoft.EntityFrameworkCore;
using ShoppingCartEntity = Mango.Services.ShoppingCart.Domain.Entities.ShoppingCart;
using Mango.Services.ShoppingCart.Application.Interfaces;
using Mango.Services.ShoppingCart.Infrastructure.Data;

namespace Mango.Services.ShoppingCart.Infrastructure.Repositories;

/// <summary>
/// Shopping Cart repository implementation with EF Core.
/// Handles all data access operations for shopping carts.
/// </summary>
public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly ShoppingCartDbContext _context;

    public ShoppingCartRepository(ShoppingCartDbContext context)
    {
        _context = context;
    }

    public async Task<ShoppingCartEntity?> GetByIdAsync(int id)
    {
        return await _context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ShoppingCartEntity?> GetByUserIdAsync(string userId)
    {
        return await _context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task AddAsync(ShoppingCartEntity cart)
    {
        cart.CreatedAt = DateTime.UtcNow;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.ShoppingCarts.AddAsync(cart);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(ShoppingCartEntity cart)
    {
        var existing = await _context.ShoppingCarts.FindAsync(cart.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Shopping cart with ID {cart.Id} not found");
        }

        cart.UpdatedAt = DateTime.UtcNow;
        _context.ShoppingCarts.Update(cart);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var cart = await _context.ShoppingCarts.FindAsync(id);
        if (cart != null)
        {
            _context.ShoppingCarts.Remove(cart);
            await SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await GetByUserIdAsync(userId);
        if (cart != null)
        {
            cart.Clear();
            await UpdateAsync(cart);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
