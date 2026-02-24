using MediatR;

namespace Mango.Services.Order.Application.MediatR;

/// <summary>
/// Base class for all queries.
/// </summary>
public abstract class BaseQuery<TResponse> : IRequest<TResponse>
{
}
