namespace Mango.Services.Payment.Application.MediatR.Commands;

using Mango.Services.Payment.Application.DTOs;

/// <summary>
/// Command to initiate a new payment.
/// </summary>
public class InitiatePaymentCommand : BaseCommand<PaymentDto>
{
    /// <summary>
    /// Payment initiation request details.
    /// </summary>
    public PaymentInitiateRequest Request { get; set; } = null!;

    /// <summary>
    /// User ID making the request.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    public InitiatePaymentCommand(PaymentInitiateRequest request, string userId)
    {
        Request = request;
        UserId = userId;
    }
}
