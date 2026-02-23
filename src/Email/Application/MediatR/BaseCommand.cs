using MediatR;

namespace Mango.Services.Email.Application.MediatR;

/// <summary>
/// Base class for all CQRS commands with void response.
/// </summary>
public abstract class BaseCommand : IRequest { }

/// <summary>
/// Base class for all CQRS commands with typed response.
/// </summary>
public abstract class BaseCommand<TResponse> : IRequest<TResponse> { }
