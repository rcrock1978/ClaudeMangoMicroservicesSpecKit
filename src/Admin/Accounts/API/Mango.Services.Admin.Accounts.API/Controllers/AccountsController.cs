using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.MediatR.Queries;
using Mango.Services.Admin.Accounts.Application.MediatR.Commands;
using Mango.Services.Admin.Accounts.API.Middleware;
using Mango.Services.Admin.Accounts.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Mango.Services.Admin.Accounts.API.Controllers;

/// <summary>
/// API controller for admin account management.
/// Provides endpoints for creating, updating, and managing admin users and their API keys.
/// Requires X-API-Key header for authentication on most endpoints.
/// </summary>
[ApiController]
[Route("api/admin/accounts")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IMediator mediator, ILogger<AccountsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates an API key and returns admin user information.
    /// </summary>
    /// <param name="request">The validation request containing the API key.</param>
    /// <returns>Admin user information if valid.</returns>
    [HttpPost("validate")]
    [SkipApiKeyAuth]
    [ProducesResponseType(typeof(ResponseDto<ValidateApiKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto<ValidateApiKeyResponse>>> ValidateApiKey([FromBody] ValidateApiKeyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ResponseDto
            {
                IsSuccess = false,
                Message = "Invalid request"
            });
        }

        try
        {
            var query = new ValidateApiKeyQuery(request.ApiKey);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess || result.Result?.IsValid != true)
            {
                return Unauthorized(new ResponseDto<ValidateApiKeyResponse>
                {
                    IsSuccess = false,
                    Message = result.Message ?? "API key validation failed"
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                IsSuccess = false,
                Message = $"Error validating API key: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Creates a new admin user (SUPER_ADMIN only).
    /// </summary>
    /// <param name="request">The admin user creation request.</param>
    /// <returns>The created admin user information.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<AdminUserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ResponseDto<AdminUserDto>>> CreateAdminUser([FromBody] CreateAdminUserRequest request)
    {
        try
        {
            // Verify admin role from claims
            var roleClaimValue = User.FindFirst("AdminRole")?.Value;
            if (roleClaimValue == null || !Enum.TryParse<AdminRole>(roleClaimValue, out var userRole) || userRole != AdminRole.SUPER_ADMIN)
            {
                _logger.LogWarning("Unauthorized attempt to create admin user from {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return Forbid();
            }

            var command = new CreateAdminUserCommand
            {
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role
            };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Admin user created: {Email} with role {Role}", request.Email, request.Role);
            return CreatedAtAction(nameof(GetAdminUserById), new { id = result.Result?.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin user");
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                IsSuccess = false,
                Message = $"Error creating admin user: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Gets a specific admin user by ID.
    /// </summary>
    /// <param name="id">The admin user ID.</param>
    /// <returns>Admin user information.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseDto<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto<AdminUserDto>>> GetAdminUserById(int id)
    {
        try
        {
            var query = new GetAdminUserByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                IsSuccess = false,
                Message = $"Error retrieving admin user: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Lists all admin users with pagination (ADMIN+ only).
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10, max: 100).</param>
    /// <returns>Paginated list of admin users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PaginatedResult<AdminUserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ResponseDto<PaginatedResult<AdminUserDto>>>> ListAdminUsers(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            // Verify admin+ role
            var roleClaimValue = User.FindFirst("AdminRole")?.Value;
            if (roleClaimValue == null || !Enum.TryParse<AdminRole>(roleClaimValue, out var userRole)
                || (userRole != AdminRole.SUPER_ADMIN && userRole != AdminRole.ADMIN))
            {
                _logger.LogWarning("Unauthorized access to admin list from {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return Forbid();
            }

            // Validate pagination
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = new GetAllAdminUsersQuery { PageNumber = pageNumber, PageSize = pageSize };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing admin users");
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                IsSuccess = false,
                Message = $"Error listing admin users: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Updates an admin user (SUPER_ADMIN or self only).
    /// </summary>
    /// <param name="id">The admin user ID.</param>
    /// <param name="request">The update request.</param>
    /// <returns>Updated admin user information.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseDto<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDto<AdminUserDto>>> UpdateAdminUser(int id, [FromBody] UpdateAdminUserRequest request)
    {
        try
        {
            // Verify authorization: SUPER_ADMIN or self
            var currentUserId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 0;
            var roleClaimValue = User.FindFirst("AdminRole")?.Value;

            if (!Enum.TryParse<AdminRole>(roleClaimValue, out var userRole)
                || (userRole != AdminRole.SUPER_ADMIN && currentUserId != id))
            {
                _logger.LogWarning("Unauthorized attempt to update admin user {TargetId} from {UserId}", id, currentUserId);
                return Forbid();
            }

            var command = new UpdateAdminUserCommand
            {
                Id = id,
                FullName = request.FullName,
                Role = request.Role,
                IsActive = request.IsActive
            };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.Result == null ? NotFound(result) : BadRequest(result);
            }

            _logger.LogInformation("Admin user {UserId} updated", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                IsSuccess = false,
                Message = $"Error updating admin user: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Deactivates (soft delete) an admin user (SUPER_ADMIN only).
    /// </summary>
    /// <param name="id">The admin user ID.</param>
    /// <returns>Deactivation response.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDto>> DeactivateAdminUser(int id)
    {
        try
        {
            // Verify SUPER_ADMIN role
            var roleClaimValue = User.FindFirst("AdminRole")?.Value;
            if (roleClaimValue == null || !Enum.TryParse<AdminRole>(roleClaimValue, out var userRole) || userRole != AdminRole.SUPER_ADMIN)
            {
                _logger.LogWarning("Unauthorized attempt to deactivate admin user from {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return Forbid();
            }

            var command = new DeactivateAdminUserCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.Result == null ? NotFound(result) : BadRequest(result);
            }

            _logger.LogInformation("Admin user {UserId} deactivated", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating admin user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                IsSuccess = false,
                Message = $"Error deactivating admin user: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Creates a new API key for an admin user (user's own key or SUPER_ADMIN can create for others).
    /// Returns the plain text key once (must be saved by client).
    /// </summary>
    /// <param name="id">The admin user ID.</param>
    /// <returns>New API key (plain text, shown only once).</returns>
    [HttpPost("{id}/api-keys")]
    [ProducesResponseType(typeof(ResponseDto<CreateApiKeyResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDto<CreateApiKeyResponse>>> CreateApiKey(int id)
    {
        try
        {
            // Verify authorization: own key or SUPER_ADMIN
            var currentUserId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 0;
            var roleClaimValue = User.FindFirst("AdminRole")?.Value;

            if (!Enum.TryParse<AdminRole>(roleClaimValue, out var userRole)
                || (userRole != AdminRole.SUPER_ADMIN && currentUserId != id))
            {
                _logger.LogWarning("Unauthorized attempt to create API key for user {TargetId} from {UserId}", id, currentUserId);
                return Forbid();
            }

            var command = new CreateApiKeyCommand { AdminUserId = id };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.Result == null ? NotFound(result) : BadRequest(result);
            }

            _logger.LogInformation("New API key created for admin user {UserId}", id);
            return CreatedAtAction(nameof(GetAdminUserById), new { id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API key for user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                IsSuccess = false,
                Message = $"Error creating API key: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Revokes (disables) the current API key for an admin user (user's own key or SUPER_ADMIN).
    /// </summary>
    /// <param name="id">The admin user ID.</param>
    /// <returns>Revocation response.</returns>
    [HttpDelete("{id}/api-keys")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDto>> RevokeApiKey(int id)
    {
        try
        {
            // Verify authorization: own key or SUPER_ADMIN
            var currentUserId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 0;
            var roleClaimValue = User.FindFirst("AdminRole")?.Value;

            if (!Enum.TryParse<AdminRole>(roleClaimValue, out var userRole)
                || (userRole != AdminRole.SUPER_ADMIN && currentUserId != id))
            {
                _logger.LogWarning("Unauthorized attempt to revoke API key for user {TargetId} from {UserId}", id, currentUserId);
                return Forbid();
            }

            var command = new RevokeApiKeyCommand { AdminUserId = id };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.Result == null ? NotFound(result) : BadRequest(result);
            }

            _logger.LogInformation("API key revoked for admin user {UserId}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking API key for user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                IsSuccess = false,
                Message = $"Error revoking API key: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    [SkipApiKeyAuth]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

/// <summary>
/// Response model for API key creation (plain text key).
/// </summary>
public class CreateApiKeyResponse
{
    public string PlainTextKey { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Paginated result wrapper for list responses.
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
