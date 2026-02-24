namespace Mango.Services.Admin.Accounts.Application.DTOs;

/// <summary>
/// Standard response wrapper for API responses.
/// </summary>
public class ResponseDto
{
    public bool IsSuccess { get; set; } = true;
    public object? Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ErrorCode { get; set; }
}

/// <summary>
/// Generic standard response wrapper for API responses.
/// </summary>
/// <typeparam name="T">The type of the result data.</typeparam>
public class ResponseDto<T>
{
    public bool IsSuccess { get; set; } = true;
    public T? Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ErrorCode { get; set; }
}

/// <summary>
/// Paginated response wrapper.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
