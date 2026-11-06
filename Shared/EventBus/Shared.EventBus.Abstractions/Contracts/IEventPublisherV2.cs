// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           IEventPublisherV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.4: publisher-side abstraction for EventEnvelopeV2.
//                  Deliberately a separate interface from IEventPublisher
//                  (V1), not an overload of it -- the two envelope types are
//                  not interchangeable (V2 carries enrichment V1 never
//                  will), and callers must be explicit about which stream
//                  they're publishing to.
// ============================================================================
namespace Shared.EventBus.Abstractions.Contracts;

/// <summary>Publishes V2 (enriched) cross-module integration events.</summary>
public interface IEventPublisherV2
{
    /// <summary>Enqueues the given V2 envelope for delivery to all registered V2 subscribers.</summary>
    Task PublishAsync(EventEnvelopeV2 envelope, CancellationToken cancellationToken = default);

    /// <summary>Convenience overload: builds and publishes a V2 envelope from a typed DTO plus its enrichment fields.</summary>
    Task PublishAsync<TPayload>(
        TPayload payload,
        string tenantId,
        Guid correlationId,
        Guid causationId,
        DateTime timestampUtc,
        string sourceModule,
        string eventType,
        int schemaVersion,
        Guid? facilityId = null,
        string? industryName = null,
        string? location = null,
        string? hazardType = null,
        CancellationToken cancellationToken = default)
        where TPayload : notnull;
}
