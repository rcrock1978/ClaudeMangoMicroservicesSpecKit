using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mango.Services.Admin.Application.MediatR.Queries;

namespace Mango.Services.Admin.API.Controllers;

/// <summary>
/// Controller for admin dashboard KPI endpoints.
/// Requires API key authentication via X-API-Key header.
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets comprehensive KPI dashboard with all metrics aggregated from microservices.
    /// </summary>
    /// <param name="startDate">Optional start date for metrics (ISO 8601 format).</param>
    /// <param name="endDate">Optional end date for metrics (ISO 8601 format).</param>
    /// <param name="detailed">Include detailed metric breakdowns (default: true).</param>
    /// <returns>Complete dashboard KPI data.</returns>
    [HttpGet("kpis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardKpis(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool detailed = true)
    {
        try
        {
            _logger.LogInformation(
                "Dashboard KPI request from {AdminId}: startDate={StartDate}, endDate={EndDate}, detailed={Detailed}",
                User.FindFirst("AdminId")?.Value ?? "unknown",
                startDate?.ToString("yyyy-MM-dd"),
                endDate?.ToString("yyyy-MM-dd"),
                detailed);

            // Validate date range
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new { error = "Start date cannot be after end date" });
            }

            // Limit date range to 2 years for performance
            if (startDate.HasValue && (DateTime.UtcNow - startDate.Value).TotalDays > 730)
            {
                return BadRequest(new { error = "Date range cannot exceed 2 years" });
            }

            var query = new GetDashboardKpisQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                IncludeDetailedBreakdown = detailed
            };

            var dashboard = await _mediator.Send(query);

            if (dashboard == null)
            {
                _logger.LogWarning("Failed to retrieve dashboard KPIs");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "Unable to retrieve dashboard data. Please try again later." });
            }

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard KPIs");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving dashboard data" });
        }
    }

    /// <summary>
    /// Gets revenue metrics including daily/monthly breakdown and trends.
    /// </summary>
    /// <param name="startDate">Start date for revenue metrics (ISO 8601 format).</param>
    /// <param name="endDate">End date for revenue metrics (ISO 8601 format).</param>
    /// <returns>Revenue metrics with trend analysis.</returns>
    [HttpGet("revenue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRevenueMetrics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest(new { error = "Start date must be before end date" });
            }

            _logger.LogInformation(
                "Revenue metrics request from {AdminId}: {StartDate} to {EndDate}",
                User.FindFirst("AdminId")?.Value ?? "unknown",
                startDate.ToString("O"),
                endDate.ToString("O"));

            var query = new GetRevenueMetricsQuery { StartDate = startDate, EndDate = endDate };
            var metrics = await _mediator.Send(query);

            if (metrics == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "Unable to retrieve revenue metrics" });
            }

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving revenue metrics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving revenue metrics" });
        }
    }

    /// <summary>
    /// Gets product metrics including inventory and performance data.
    /// </summary>
    /// <returns>Product metrics with category and stock analysis.</returns>
    [HttpGet("products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductMetrics()
    {
        try
        {
            _logger.LogInformation("Product metrics request from {AdminId}",
                User.FindFirst("AdminId")?.Value ?? "unknown");

            var query = new GetProductMetricsQuery();
            var metrics = await _mediator.Send(query);

            if (metrics == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "Unable to retrieve product metrics" });
            }

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product metrics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving product metrics" });
        }
    }

    /// <summary>
    /// Gets customer insights including demographics, engagement, and lifetime value.
    /// </summary>
    /// <returns>Customer analytics and segmentation data.</returns>
    [HttpGet("customers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCustomerInsights()
    {
        try
        {
            _logger.LogInformation("Customer insights request from {AdminId}",
                User.FindFirst("AdminId")?.Value ?? "unknown");

            var query = new GetCustomerInsightsQuery();
            var metrics = await _mediator.Send(query);

            if (metrics == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "Unable to retrieve customer insights" });
            }

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer insights");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving customer insights" });
        }
    }

    /// <summary>
    /// Gets coupon analytics including usage, performance, and redemption rates.
    /// </summary>
    /// <returns>Coupon campaign analytics and effectiveness metrics.</returns>
    [HttpGet("coupons")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCouponAnalytics()
    {
        try
        {
            _logger.LogInformation("Coupon analytics request from {AdminId}",
                User.FindFirst("AdminId")?.Value ?? "unknown");

            var query = new GetCouponAnalyticsQuery();
            var metrics = await _mediator.Send(query);

            if (metrics == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "Unable to retrieve coupon analytics" });
            }

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving coupon analytics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving coupon analytics" });
        }
    }

    /// <summary>
    /// Health check endpoint to verify service is operational.
    /// </summary>
    /// <returns>Service status.</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        _logger.LogInformation("Health check request");
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "Mango.Services.Admin"
        });
    }
}
