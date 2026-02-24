namespace Mango.Services.Payment.Application.MediatR.Queries;

using Mango.Services.Payment.Application.DTOs;

/// <summary>
/// Query to get payment history for a user.
/// </summary>
public class GetPaymentHistoryQuery : BaseQuery<List<PaymentDto>>
{
    /// <summary>
    /// User ID to retrieve payment history for.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Number of records to skip.
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// Number of records to take.
    /// </summary>
    public int Take { get; set; } = 10;

    public GetPaymentHistoryQuery(string userId, int skip = 0, int take = 10)
    {
        UserId = userId;
        Skip = skip;
        Take = take;
    }
}
