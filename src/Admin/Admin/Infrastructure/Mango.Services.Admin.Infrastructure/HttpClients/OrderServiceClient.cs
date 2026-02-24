using Serilog;

namespace Mango.Services.Admin.Infrastructure.HttpClients;

/// <summary>
/// HTTP client for communicating with the Order microservice.
/// </summary>
public interface IOrderServiceClient
{
    /// <summary>Gets all orders with optional filtering.</summary>
    Task<List<OrderDto>?> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 50);

    /// <summary>Gets a single order by ID.</summary>
    Task<OrderDto?> GetOrderByIdAsync(string id);

    /// <summary>Gets total order count.</summary>
    Task<int> GetTotalOrderCountAsync();

    /// <summary>Gets orders by status.</summary>
    Task<List<OrderDto>?> GetOrdersByStatusAsync(string status, int pageNumber = 1, int pageSize = 50);

    /// <summary>Gets orders within a date range.</summary>
    Task<List<OrderDto>?> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>Gets orders for a specific customer.</summary>
    Task<List<OrderDto>?> GetCustomerOrdersAsync(string userId);

    /// <summary>Gets summary statistics for orders.</summary>
    Task<OrderStatsDto?> GetOrderStatsAsync();
}

public class OrderServiceClient : BaseServiceClient, IOrderServiceClient
{
    public OrderServiceClient(HttpClient httpClient, ILogger logger)
        : base(httpClient, logger)
    {
    }

    public async Task<List<OrderDto>?> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 50)
    {
        _logger.Information("Fetching orders: page {PageNumber}, size {PageSize}", pageNumber, pageSize);
        return await GetAsync<List<OrderDto>>($"/api/orders?pageNumber={pageNumber}&pageSize={pageSize}");
    }

    public async Task<OrderDto?> GetOrderByIdAsync(string id)
    {
        _logger.Information("Fetching order {OrderId}", id);
        return await GetAsync<OrderDto>($"/api/orders/{id}");
    }

    public async Task<int> GetTotalOrderCountAsync()
    {
        _logger.Information("Fetching total order count");
        var result = await GetAsync<dynamic>("/api/orders/count");
        return result?.count ?? 0;
    }

    public async Task<List<OrderDto>?> GetOrdersByStatusAsync(string status, int pageNumber = 1, int pageSize = 50)
    {
        _logger.Information("Fetching orders with status {Status}: page {PageNumber}", status, pageNumber);
        return await GetAsync<List<OrderDto>>($"/api/orders/status/{status}?pageNumber={pageNumber}&pageSize={pageSize}");
    }

    public async Task<List<OrderDto>?> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.Information("Fetching orders between {StartDate} and {EndDate}", startDate, endDate);
        var startStr = startDate.ToString("O");
        var endStr = endDate.ToString("O");
        return await GetAsync<List<OrderDto>>($"/api/orders/date-range?startDate={startStr}&endDate={endStr}");
    }

    public async Task<List<OrderDto>?> GetCustomerOrdersAsync(string userId)
    {
        _logger.Information("Fetching orders for customer {UserId}", userId);
        return await GetAsync<List<OrderDto>>($"/api/orders/customer/{userId}");
    }

    public async Task<OrderStatsDto?> GetOrderStatsAsync()
    {
        _logger.Information("Fetching order statistics");
        return await GetAsync<OrderStatsDto>("/api/orders/stats");
    }
}

/// <summary>DTO for order data from Order service.</summary>
public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>DTO for order items.</summary>
public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>DTO for order statistics.</summary>
public class OrderStatsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int OrdersThisMonth { get; set; }
    public int OrdersThisWeek { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
}
