// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           OutboxPipelineBehavior.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MediatR pipeline behavior that writes domain events to the outbox after command execution.
//                  Additive-only evolution.
// ============================================================================
using MediatR;

namespace PlantSafety.Infrastructure.Common.Pipelines;

/// <summary>
/// MediatR pipeline behavior that intercepts command handlers after successful execution
/// and writes any raised domain events to the <c>OutboxMessages</c> table within the
/// same SQL transaction (Publish-After-Commit guarantee).
/// </summary>
public sealed class OutboxPipelineBehavior<TRequest, TResponse>(
    IOutboxWriter outboxWriter,
    ILogger<OutboxPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        // Outbox writing is handled within the UoW transaction; this hook
        // triggers final flush if the UoW didn't already write.
        if (outboxWriter.HasPendingEvents)
        {
            await outboxWriter.FlushAsync(cancellationToken);
            logger.LogDebug("OutboxPipelineBehavior: flushed pending events.");
        }

        return response;
    }
}

/// <summary>Abstraction over the outbox write mechanism.</summary>
public interface IOutboxWriter
{
    /// <summary>Returns <see langword="true"/> when there are un-flushed domain events.</summary>
    bool HasPendingEvents { get; }

    /// <summary>Writes all pending domain events to the outbox table.</summary>
    Task FlushAsync(CancellationToken cancellationToken = default);
}
