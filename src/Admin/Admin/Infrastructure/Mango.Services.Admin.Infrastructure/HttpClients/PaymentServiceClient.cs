using Serilog;

namespace Mango.Services.Admin.Infrastructure.HttpClients;

/// <summary>
/// HTTP client for communicating with the Payment microservice.
/// </summary>
public interface IPaymentServiceClient
{
    /// <summary>Gets revenue metrics for a date range.</summary>
    Task<RevenueMetricsDto?> GetRevenueMetricsAsync(DateTime startDate, DateTime endDate);

    /// <summary>Gets payment status summary.</summary>
    Task<PaymentSummaryDto?> GetPaymentSummaryAsync();

    /// <summary>Gets refunds within a date range.</summary>
    Task<List<RefundDto>?> GetRefundsAsync(DateTime startDate, DateTime endDate);

    /// <summary>Gets daily revenue breakdown.</summary>
    Task<List<DailyRevenueDto>?> GetDailyRevenueAsync(DateTime startDate, DateTime endDate);

    /// <summary>Gets payment method breakdown.</summary>
    Task<List<PaymentMethodBreakdownDto>?> GetPaymentMethodBreakdownAsync(DateTime startDate, DateTime endDate);
}

public class PaymentServiceClient : BaseServiceClient, IPaymentServiceClient
{
    public PaymentServiceClient(HttpClient httpClient, ILogger logger)
        : base(httpClient, logger)
    {
    }

    public async Task<RevenueMetricsDto?> GetRevenueMetricsAsync(DateTime startDate, DateTime endDate)
    {
        _logger.Information("Fetching revenue metrics from {StartDate} to {EndDate}", startDate, endDate);
        var startStr = startDate.ToString("O");
        var endStr = endDate.ToString("O");
        return await GetAsync<RevenueMetricsDto>($"/api/payments/revenue?startDate={startStr}&endDate={endStr}");
    }

    public async Task<PaymentSummaryDto?> GetPaymentSummaryAsync()
    {
        _logger.Information("Fetching payment summary");
        return await GetAsync<PaymentSummaryDto>("/api/payments/summary");
    }

    public async Task<List<RefundDto>?> GetRefundsAsync(DateTime startDate, DateTime endDate)
    {
        _logger.Information("Fetching refunds from {StartDate} to {EndDate}", startDate, endDate);
        var startStr = startDate.ToString("O");
        var endStr = endDate.ToString("O");
        return await GetAsync<List<RefundDto>>($"/api/payments/refunds?startDate={startStr}&endDate={endStr}");
    }

    public async Task<List<DailyRevenueDto>?> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
    {
        _logger.Information("Fetching daily revenue from {StartDate} to {EndDate}", startDate, endDate);
        var startStr = startDate.ToString("O");
        var endStr = endDate.ToString("O");
        return await GetAsync<List<DailyRevenueDto>>($"/api/payments/daily-revenue?startDate={startStr}&endDate={endStr}");
    }

    public async Task<List<PaymentMethodBreakdownDto>?> GetPaymentMethodBreakdownAsync(DateTime startDate, DateTime endDate)
    {
        _logger.Information("Fetching payment method breakdown from {StartDate} to {EndDate}", startDate, endDate);
        var startStr = startDate.ToString("O");
        var endStr = endDate.ToString("O");
        return await GetAsync<List<PaymentMethodBreakdownDto>>($"/api/payments/method-breakdown?startDate={startStr}&endDate={endStr}");
    }
}

/// <summary>DTO for revenue metrics.</summary>
public class RevenueMetricsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal NetRevenue => TotalRevenue - TotalRefunds;
}

/// <summary>DTO for payment summary.</summary>
public class PaymentSummaryDto
{
    public decimal TodayRevenue { get; set; }
    public decimal ThisWeekRevenue { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public int PendingPayments { get; set; }
    public int ProcessedPayments { get; set; }
    public int FailedPayments { get; set; }
}

/// <summary>DTO for refund data.</summary>
public class RefundDto
{
    public string Id { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>DTO for daily revenue breakdown.</summary>
public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

/// <summary>DTO for payment method breakdown.</summary>
public class PaymentMethodBreakdownDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}
