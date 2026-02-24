using MediatR;

namespace Mango.Services.Admin.Application.MediatR;

/// <summary>
/// Base class for commands that don't return a value.
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
