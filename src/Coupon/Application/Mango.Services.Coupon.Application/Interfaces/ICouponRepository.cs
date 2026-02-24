using CouponEntity = Mango.Services.Coupon.Domain.Entities.Coupon;

namespace Mango.Services.Coupon.Application.Interfaces;

/// <summary>
/// Repository interface for Coupon data access.
/// Implements repository pattern with async operations.
/// </summary>
public interface ICouponRepository
{
    /// <summary>
    /// Get coupon by ID.
    /// </summary>
    Task<CouponEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Get coupon by code (case-insensitive).
    /// </summary>
    Task<CouponEntity?> GetByCodeAsync(string code);

    /// <summary>
    /// Get all active coupons with pagination.
    /// </summary>
    Task<(List<CouponEntity> Items, int TotalCount)> GetAllActiveAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Get all coupons (including inactive) with pagination.
    /// </summary>
    Task<(List<CouponEntity> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Get coupons valid at a specific time with pagination.
    /// </summary>
    Task<(List<CouponEntity> Items, int TotalCount)> GetValidAtTimeAsync(
        DateTime dateTime,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// Add new coupon.
    /// </summary>
    Task AddAsync(CouponEntity coupon);

    /// <summary>
    /// Update existing coupon.
    /// </summary>
    Task UpdateAsync(CouponEntity coupon);

    /// <summary>
    /// Delete coupon by ID.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Check if coupon code already exists.
    /// </summary>
    Task<bool> CodeExistsAsync(string code);

    /// <summary>
    /// Save changes to database.
    /// </summary>
    Task SaveChangesAsync();
}
