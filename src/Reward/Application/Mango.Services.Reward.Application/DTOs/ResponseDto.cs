namespace Mango.Services.Reward.Application.DTOs;

public class ResponseDto
{
    public bool IsSuccess { get; set; } = true;
    public object? Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ErrorCode { get; set; }

    public static ResponseDto Success(object? result = null, string message = "Operation successful")
        => new() { IsSuccess = true, Result = result, Message = message };

    public static ResponseDto Error(string message, int? errorCode = null)
        => new() { IsSuccess = false, Message = message, ErrorCode = errorCode };
}

public class ResponseDto<T>
{
    public bool IsSuccess { get; set; } = true;
    public T? Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ErrorCode { get; set; }

    public static ResponseDto<T> Success(T? result, string message = "Operation successful")
        => new() { IsSuccess = true, Result = result, Message = message };

    public static ResponseDto<T> Error(string message, int? errorCode = null)
        => new() { IsSuccess = false, Message = message, ErrorCode = errorCode };
}
