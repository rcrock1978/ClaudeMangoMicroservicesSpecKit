namespace Mango.Services.Coupon.Application.DTOs;

/// <summary>
/// Data transfer object for Coupon entity in API responses.
/// </summary>
public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinimumCartValue { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxUsageCount { get; set; }
    public int CurrentUsageCount { get; set; }
    public int MaxUsagePerUser { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Request model for validating coupon application.
/// </summary>
public class ValidateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal CartTotal { get; set; }
    public int UserUsageCount { get; set; } = 0;
}

/// <summary>
/// Response model for coupon validation.
/// </summary>
public class ValidateCouponResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public CouponDto? Coupon { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalCartTotal { get; set; }
}

/// <summary>
/// Request model for applying coupon.
/// </summary>
public class ApplyCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal CartTotal { get; set; }
}

/// <summary>
/// Paginated response for coupon listings.
/// </summary>
public class PaginatedCouponResponse
{
    public List<CouponDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
