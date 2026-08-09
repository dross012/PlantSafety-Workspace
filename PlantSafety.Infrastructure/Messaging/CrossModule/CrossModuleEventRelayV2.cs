// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.CrossModule
//  Layer:          Infrastructure
//  File:           CrossModuleEventRelayV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3: ICrossModuleEventRelayV2 implementation.
//                  Buffers V2-mapped envelopes in-memory (Enqueue is
//                  synchronous, mirroring CrossModuleEventRelay); at
//                  FlushAsync, resolves facility enrichment for each
//                  buffered envelope via IFacilityInferenceResolver
//                  (injected, defaults to NullFacilityInferenceResolver --
//                  see FacilityInferenceRules.cs's header for why "unknown"
//                  is the honest answer today) and publishes via
//                  IEventPublisherV2.
// ============================================================================
using Microsoft.Extensions.Logging;

using PlantSafety.Domain.Common.Abstractions;

using Shared.EventBus.Abstractions.Contracts;

namespace PlantSafety.Infrastructure.Messaging.CrossModule;

/// <inheritdoc cref="ICrossModuleEventRelayV2"/>
public sealed class CrossModuleEventRelayV2(
    IEventPublisherV2 eventPublisherV2,
    IFacilityInferenceResolver facilityInferenceResolver,
    ILogger<CrossModuleEventRelayV2> logger) : ICrossModuleEventRelayV2
{
    private readonly List<EventEnvelopeV2> _pending = [];

    /// <inheritdoc/>
    public bool HasPendingEvents => _pending.Count > 0;

    /// <inheritdoc/>
    public void Enqueue(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId)
    {
        if (!CrossModuleDomainEventMapperV2.TryMap(domainEvent, tenantId, out var envelope) || envelope is null)
        {
            // Not every domain event has a V2 cross-module contract yet -- see
            // CrossModuleDomainEventMapperV2.cs's header. Expected for most events, not an error.
            return;
        }

        _pending.Add(envelope);
    }

    /// <inheritdoc/>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (_pending.Count == 0) return;
        logger.LogDebug("CrossModuleEventRelayV2: publishing {Count} cross-module V2 event(s) after commit.", _pending.Count);

        foreach (var envelope in _pending)
        {
            EventEnvelopeV2 enriched = await FacilityInferenceRules.EnrichWithFacilityAsync(envelope, facilityInferenceResolver, cancellationToken);
            await eventPublisherV2.PublishAsync(enriched, cancellationToken);
        }

        logger.LogDebug("CrossModuleEventRelayV2: published {Count} cross-module V2 event(s).", _pending.Count);
        _pending.Clear();
    }

    /// <inheritdoc/>
    public void Clear()
    {
        if (_pending.Count == 0) return;
        logger.LogDebug("CrossModuleEventRelayV2: discarding {Count} pending cross-module V2 event(s) (rollback).", _pending.Count);
        _pending.Clear();
    }
}
