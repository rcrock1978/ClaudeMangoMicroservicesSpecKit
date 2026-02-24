using Microsoft.EntityFrameworkCore;
using CouponEntity = Mango.Services.Coupon.Domain.Entities.Coupon;
using Mango.Services.Coupon.Application.Interfaces;
using Mango.Services.Coupon.Infrastructure.Data;

namespace Mango.Services.Coupon.Infrastructure.Repositories;

/// <summary>
/// Coupon repository implementation with EF Core.
/// Handles all data access operations for coupons.
/// </summary>
public class CouponRepository : ICouponRepository
{
    private readonly CouponDbContext _context;

    public CouponRepository(CouponDbContext context)
    {
        _context = context;
    }

    public async Task<CouponEntity?> GetByIdAsync(int id)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CouponEntity?> GetByCodeAsync(string code)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());
    }

    public async Task<(List<CouponEntity>, int)> GetAllActiveAsync(int pageNumber, int pageSize)
    {
        var now = DateTime.UtcNow;
        var query = _context.Coupons
            .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<CouponEntity>, int)> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Coupons.AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<CouponEntity>, int)> GetValidAtTimeAsync(
        DateTime dateTime,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Coupons
            .Where(c => c.IsActive && c.StartDate <= dateTime && c.EndDate >= dateTime);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(CouponEntity coupon)
    {
        coupon.CreatedAt = DateTime.UtcNow;
        coupon.UpdatedAt = DateTime.UtcNow;

        await _context.Coupons.AddAsync(coupon);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(CouponEntity coupon)
    {
        var existing = await _context.Coupons.FindAsync(coupon.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Coupon with ID {coupon.Id} not found");
        }

        coupon.UpdatedAt = DateTime.UtcNow;
        _context.Coupons.Update(coupon);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null)
        {
            throw new InvalidOperationException($"Coupon with ID {id} not found");
        }

        _context.Coupons.Remove(coupon);
        await SaveChangesAsync();
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        return await _context.Coupons
            .AnyAsync(c => c.Code.ToLower() == code.ToLower());
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
