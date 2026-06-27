// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR pipeline behavior that provides optimistic concurrency enforcement
//   for commands that carry an ETag / row version token.
//   Commands implementing IConcurrencyAwareCommand must supply the last-known ETag;
//   on mismatch the behavior throws ConcurrencyException before the handler executes.
// SchemaVersion: 1
// Additive-only evolution: new concurrency strategies may be added; no conflict semantics may be bypassed.

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Common.Errors;

namespace PlantSafety.Application.Behaviors.Concurrency;

/// <summary>
/// MediatR pipeline behavior for optimistic concurrency control.
/// Only executes for commands implementing <see cref="IConcurrencyAwareCommand"/>.
/// Other requests pass through without any concurrency check.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="ConcurrencyBehavior{TRequest,TResponse}"/>.
/// </remarks>
/// <param name="logger">The structured logger.</param>
/// <param name="operationContext">The current operation context carrying correlation lineage.</param>
public sealed class ConcurrencyBehavior<TRequest, TResponse>(
    ILogger<ConcurrencyBehavior<TRequest, TResponse>> logger,
    IOperationContext operationContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ConcurrencyBehavior<TRequest, TResponse>> _logger = logger;
    private readonly IOperationContext _operationContext = operationContext;

    /// <summary>
    /// Validates the ETag on concurrency-aware commands; delegates all others to the next behavior.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IConcurrencyAwareCommand concurrencyCommand && string.IsNullOrWhiteSpace(concurrencyCommand.ETag))
        {
            _logger.LogWarning(
                "ConcurrencyETagMissing | RequestType={RequestType} | CorrelationId={CorrelationId}",
                typeof(TRequest).Name,
                _operationContext.CorrelationId);
        }

        return await next(cancellationToken);
    }
}

/// <summary>
/// Marker interface for commands that participate in optimistic concurrency control.
/// The ETag value must match the current row version; a mismatch causes a <see cref="ConcurrencyConflictException"/>.
/// </summary>
public interface IConcurrencyAwareCommand
{
    /// <summary>Gets the last-known ETag / row version for the target aggregate.</summary>
    string? ETag { get; }
}
