// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           IEventHandlerV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.4: consumer-side handler contract for
//                  EventEnvelopeV2, mirroring IEventHandler&lt;TPayload&gt; (V1).
// ============================================================================
namespace Shared.EventBus.Abstractions.Handlers;

/// <summary>Handles a single V2 cross-module integration event payload type.</summary>
/// <typeparam name="TPayload">The deserialized integration event DTO type.</typeparam>
public interface IEventHandlerV2<in TPayload> where TPayload : notnull
{
    /// <summary>Logical name of the consumer group this handler belongs to.</summary>
    string ConsumerGroup { get; }

    /// <summary>Handles the event. MUST be idempotent -- may be invoked more than once for the same EventId under at-least-once delivery.</summary>
    Task HandleAsync(TPayload payload, Contracts.EventEnvelopeV2 envelope, CancellationToken cancellationToken);
}
