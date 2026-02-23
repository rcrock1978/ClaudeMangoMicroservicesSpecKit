using MediatR;

namespace Mango.Services.Email.Application.MediatR;

/// <summary>
/// Base class for all CQRS queries with typed response.
/// </summary>
public abstract class BaseQuery<TResponse> : IRequest<TResponse> { }
