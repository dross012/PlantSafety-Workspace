// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentCreatedEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a new HazmatShipment aggregate is created.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Events;

/// <summary>Raised when a new <see cref="Aggregates.HazmatShipment"/> aggregate is created.</summary>
public sealed record HazmatShipmentCreatedEvent(
    HazmatShipmentEventId EventId,
    HazmatShipmentId HazmatShipmentId,
    IReadOnlyList<HazmatMaterialId> MaterialIds,
    IReadOnlyList<PackagingSpecificationId> PackagingIds,
    TransportMode Mode,
    UtcTimestamp CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazmatShipmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazmatShipmentCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
