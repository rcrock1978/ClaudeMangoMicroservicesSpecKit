namespace Mango.Services.Payment.Application.MediatR;

using MediatR;
using global::MediatR;

/// <summary>
/// Base class for all MediatR commands.
/// Commands represent operations that change state.
/// </summary>
public abstract class BaseCommand : IRequest
{
}

/// <summary>
/// Base class for MediatR commands that return a response.
/// </summary>
/// <typeparam name="TResponse">Type of response returned by the command handler</typeparam>
public abstract class BaseCommand<TResponse> : IRequest<TResponse>
{
}
