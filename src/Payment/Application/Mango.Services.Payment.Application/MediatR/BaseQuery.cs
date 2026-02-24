namespace Mango.Services.Payment.Application.MediatR;

using MediatR;
using global::MediatR;

/// <summary>
/// Base class for all MediatR queries.
/// Queries represent operations that read state without changes.
/// </summary>
/// <typeparam name="TResponse">Type of response returned by the query handler</typeparam>
public abstract class BaseQuery<TResponse> : IRequest<TResponse>
{
}
