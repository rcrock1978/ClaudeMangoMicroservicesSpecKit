namespace Mango.Services.Payment.Application.MediatR.Commands;

using Mango.Services.Payment.Application.DTOs;

/// <summary>
/// Command to confirm and process a payment.
/// </summary>
public class ConfirmPaymentCommand : BaseCommand<PaymentDto>
{
    /// <summary>
    /// Payment confirmation request details.
    /// </summary>
    public PaymentConfirmRequest Request { get; set; } = null!;

    /// <summary>
    /// User ID making the request.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    public ConfirmPaymentCommand(PaymentConfirmRequest request, string userId)
    {
        Request = request;
        UserId = userId;
    }
}
