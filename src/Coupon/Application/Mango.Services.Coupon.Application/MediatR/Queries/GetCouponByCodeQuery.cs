using MediatR;
using Mango.Services.Coupon.Application.DTOs;
using Mango.Services.Coupon.Application.Interfaces;

namespace Mango.Services.Coupon.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve a coupon by its code.
/// </summary>
public class GetCouponByCodeQuery : IRequest<CouponDto?>
{
    public string Code { get; set; }

    public GetCouponByCodeQuery(string code)
    {
        Code = code;
    }
}

/// <summary>
/// Handler for GetCouponByCodeQuery.
/// </summary>
public class GetCouponByCodeQueryHandler : IRequestHandler<GetCouponByCodeQuery, CouponDto?>
{
    private readonly ICouponRepository _repository;

    public GetCouponByCodeQueryHandler(ICouponRepository repository)
    {
        _repository = repository;
    }

    public async Task<CouponDto?> Handle(GetCouponByCodeQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _repository.GetByCodeAsync(request.Code);

        if (coupon == null)
            return null;

        return MapCouponDto(coupon);
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
