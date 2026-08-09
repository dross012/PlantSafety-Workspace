// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.CrossModule
//  Layer:          Infrastructure
//  File:           ICrossModuleEventRelayV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3: V2 counterpart to ICrossModuleEventRelay,
//                  relaying the curated V2 subset (CrossModuleDomainEventMapperV2)
//                  onto Shared.EventBus's EventEnvelopeV2 stream, with
//                  facility enrichment applied at flush time. Mirrors
//                  ICrossModuleEventRelay's buffer/flush/clear shape and
//                  Publish-After-Commit discipline exactly.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Infrastructure.Messaging.CrossModule;

/// <summary>Buffers and relays cross-module-relevant domain events onto Shared.EventBus's V2 (enriched) stream.</summary>
public interface ICrossModuleEventRelayV2
{
    /// <summary>Returns <see langword="true"/> when there are un-flushed V2 cross-module events.</summary>
    bool HasPendingEvents { get; }

    /// <summary>Buffers a domain event for V2 cross-module relay if (and only if) a V2 DTO mapping is registered for its CLR type.</summary>
    void Enqueue(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId);

    /// <summary>
    /// Resolves facility enrichment for every buffered envelope and publishes all of them via
    /// IEventPublisherV2. Must be called only AFTER the originating SQL transaction has committed.
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>Discards all buffered events without publishing (called on rollback).</summary>
    void Clear();
}
