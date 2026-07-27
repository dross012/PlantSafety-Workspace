// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.CrossModule
//  Layer:          Infrastructure
//  File:           CrossModuleEventRelay.cs
//  Created:        2026-07-27
//  Description:    ICrossModuleEventRelay implementation. Buffers mapped envelopes in-memory
//                  (mirroring OutboxWriter's own buffer/flush shape) and publishes them via
//                  Shared.EventBus's IEventPublisher on FlushAsync, which UnitOfWork.
//                  CommitAsync calls AFTER the SQL transaction commits.
// ============================================================================
using Microsoft.Extensions.Logging;

using Shared.EventBus.Abstractions.Contracts;

namespace PlantSafety.Infrastructure.Messaging.CrossModule;

/// <inheritdoc cref="ICrossModuleEventRelay"/>
public sealed class CrossModuleEventRelay(IEventPublisher eventPublisher, ILogger<CrossModuleEventRelay> logger) : ICrossModuleEventRelay
{
    private readonly List<EventEnvelope> _pending = [];

    /// <inheritdoc/>
    public bool HasPendingEvents => _pending.Count > 0;

    /// <inheritdoc/>
    public void Enqueue(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId)
    {
        if (!CrossModuleDomainEventMapper.TryMap(domainEvent, tenantId, out var envelope) || envelope is null)
        {
            // Not every domain event has a cross-module contract yet -- see
            // CrossModuleDomainEventMapper's header comment. This is the expected case for
            // the vast majority of PlantSafety's ~200 domain events today, not an error.
            return;
        }

        _pending.Add(envelope);
    }

    /// <inheritdoc/>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (_pending.Count == 0) return;
        logger.LogDebug("CrossModuleEventRelay: publishing {Count} cross-module event(s) after commit.", _pending.Count);

        foreach (var envelope in _pending)
        {
            await eventPublisher.PublishAsync(envelope, cancellationToken);
        }

        logger.LogDebug("CrossModuleEventRelay: published {Count} cross-module event(s).", _pending.Count);
        _pending.Clear();
    }

    /// <inheritdoc/>
    public void Clear()
    {
        if (_pending.Count == 0) return;
        logger.LogDebug("CrossModuleEventRelay: discarding {Count} pending cross-module event(s) (rollback).", _pending.Count);
        _pending.Clear();
    }
}
