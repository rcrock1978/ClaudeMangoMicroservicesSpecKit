using MediatR;

namespace Mango.Services.Admin.Application.MediatR;

/// <summary>
/// Base class for queries.
/// </summary>
public abstract class BaseQuery<TResponse> : IRequest<TResponse>
{
}
