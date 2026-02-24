using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;

namespace Mango.Services.Admin.Accounts.Application.MediatR;

/// <summary>
/// Base class for queries that return ResponseDto<TResponse>.
/// </summary>
/// <typeparam name="TResponse">The inner response type (wrapped in ResponseDto).</typeparam>
public abstract class BaseQuery<TResponse> : IRequest<ResponseDto<TResponse>>
{
}
