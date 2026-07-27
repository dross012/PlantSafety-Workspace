// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.CrossModule
//  Layer:          Infrastructure
//  File:           ICrossModuleEventRelay.cs
//  Created:        2026-07-27
//  Description:    Relays a subset of PlantSafety's real domain events (those with a
//                  registered Shared.DomainEvents.Abstractions DTO) onto the cross-module
//                  Shared.EventBus, deliberately SEPARATE from:
//                    - Shared.AuditLogging (a different concern: who-changed-what audit
//                      trail, not cross-module integration events), and
//                    - PlantSafety's own intra-module Outbox -> Azure Service Bus pipeline
//                      (IOutboxWriter / ServiceBusEventPublisher), which remains untouched.
//                  Mirrors IOutboxWriter's buffer/flush/clear shape so UnitOfWork can drive
//                  both sinks side-by-side with the same Publish-After-Commit discipline.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.CrossModule;

/// <summary>Buffers and relays cross-module-relevant domain events onto Shared.EventBus.</summary>
public interface ICrossModuleEventRelay
{
    /// <summary>Returns <see langword="true"/> when there are un-flushed cross-module events.</summary>
    bool HasPendingEvents { get; }

    /// <summary>
    /// Buffers a domain event for cross-module relay if (and only if) a DTO mapping is
    /// registered for its CLR type. Domain events with no registered mapping are silently
    /// ignored here (they may still be intra-module-only facts with no cross-module contract).
    /// </summary>
    void Enqueue(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId);

    /// <summary>
    /// Publishes all buffered cross-module envelopes via IEventPublisher. Must be called only
    /// AFTER the originating SQL transaction has committed (true Publish-After-Commit; see
    /// UnitOfWork.CommitAsync), so no cross-module event is ever published for a change that
    /// did not durably persist.
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>Discards all buffered events without publishing (called on rollback).</summary>
    void Clear();
}
