using MediatR;
using Mango.Services.Coupon.Application.DTOs;
using Mango.Services.Coupon.Application.Interfaces;

namespace Mango.Services.Coupon.Application.MediatR.Queries;

/// <summary>
/// Query to validate if a coupon can be applied to a cart.
/// </summary>
public class ValidateCouponQuery : IRequest<ValidateCouponResponse>
{
    public string Code { get; set; }
    public decimal CartTotal { get; set; }
    public int UserUsageCount { get; set; } = 0;

    public ValidateCouponQuery(string code, decimal cartTotal, int userUsageCount = 0)
    {
        Code = code;
        CartTotal = cartTotal;
        UserUsageCount = userUsageCount;
    }
}

/// <summary>
/// Handler for ValidateCouponQuery.
/// </summary>
public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, ValidateCouponResponse>
{
    private readonly ICouponRepository _repository;

    public ValidateCouponQueryHandler(ICouponRepository repository)
    {
        _repository = repository;
    }

    public async Task<ValidateCouponResponse> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _repository.GetByCodeAsync(request.Code);

        if (coupon == null)
        {
            return new ValidateCouponResponse
            {
                IsValid = false,
                Message = "Coupon not found"
            };
        }

        // Validate coupon
        var (isValid, errorMessage) = coupon.ValidateForApplication(request.CartTotal, request.UserUsageCount);

        if (!isValid)
        {
            return new ValidateCouponResponse
            {
                IsValid = false,
                Message = errorMessage,
                Coupon = MapCouponDto(coupon)
            };
        }

        // Calculate discount
        var discountAmount = coupon.CalculateDiscount(request.CartTotal);
        var finalTotal = request.CartTotal - discountAmount;

        return new ValidateCouponResponse
        {
            IsValid = true,
            Message = "Coupon is valid and can be applied",
            Coupon = MapCouponDto(coupon),
            DiscountAmount = discountAmount,
            FinalCartTotal = finalTotal
        };
    }

    private static CouponDto MapCouponDto(dynamic coupon)
    {
        return new CouponDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            MinimumCartValue = coupon.MinimumCartValue,
            MaximumDiscountAmount = coupon.MaximumDiscountAmount,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            MaxUsageCount = coupon.MaxUsageCount,
            CurrentUsageCount = coupon.CurrentUsageCount,
            MaxUsagePerUser = coupon.MaxUsagePerUser,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt,
            UpdatedAt = coupon.UpdatedAt,
            CreatedBy = coupon.CreatedBy,
            UpdatedBy = coupon.UpdatedBy
        };
    }
}
