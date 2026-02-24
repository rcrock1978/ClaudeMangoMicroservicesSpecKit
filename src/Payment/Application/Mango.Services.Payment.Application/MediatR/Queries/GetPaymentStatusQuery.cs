namespace Mango.Services.Payment.Application.MediatR.Queries;

using Mango.Services.Payment.Application.DTOs;

/// <summary>
/// Query to get current payment status.
/// </summary>
public class GetPaymentStatusQuery : BaseQuery<PaymentStatusResponse>
{
    /// <summary>
    /// Payment ID to retrieve status for.
    /// </summary>
    public int PaymentId { get; set; }

    /// <summary>
    /// User ID making the request (for authorization).
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    public GetPaymentStatusQuery(int paymentId, string userId)
    {
        PaymentId = paymentId;
        UserId = userId;
    }
}
