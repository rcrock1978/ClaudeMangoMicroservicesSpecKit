using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;

namespace Mango.Services.Admin.Accounts.Application.MediatR;

/// <summary>
/// Base class for commands that don't return a value.
/// </summary>
public abstract class BaseCommand : IRequest
{
}

/// <summary>
/// Base class for commands that return ResponseDto<TResponse>.
/// </summary>
/// <typeparam name="TResponse">The inner response type (wrapped in ResponseDto).</typeparam>
public abstract class BaseCommand<TResponse> : IRequest<ResponseDto<TResponse>>
{
}
