namespace Mango.Services.Payment.Application.MediatR.Commands;

using Mango.Services.Payment.Application.DTOs;

/// <summary>
/// Command to refund a payment.
/// </summary>
public class RefundPaymentCommand : BaseCommand<PaymentDto>
{
    /// <summary>
    /// Refund request details.
    /// </summary>
    public RefundRequest Request { get; set; } = null!;

    /// <summary>
    /// User ID making the refund request.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    public RefundPaymentCommand(RefundRequest request, string userId)
    {
        Request = request;
        UserId = userId;
    }
}
