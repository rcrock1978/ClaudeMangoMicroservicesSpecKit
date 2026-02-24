using MediatR;

namespace Mango.Services.Order.Application.MediatR;

/// <summary>
/// Base class for all commands.
/// </summary>
public abstract class BaseCommand : IRequest
{
}

/// <summary>
/// Base class for commands that return a response.
/// </summary>
public abstract class BaseCommand<TResponse> : IRequest<TResponse>
{
}
