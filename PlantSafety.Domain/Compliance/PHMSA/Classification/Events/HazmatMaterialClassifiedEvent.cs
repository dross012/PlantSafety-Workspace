// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialClassifiedEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a new HazmatMaterial aggregate is created and classified.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Events;

/// <summary>Raised when a new <c>HazmatMaterial</c> aggregate is created and classified.</summary>
public sealed record HazmatMaterialClassifiedEvent(
    HazmatMaterialId  HazmatMaterialId,
    UNNumber          UNNumber,
    ProperShippingName ProperShippingName,
    HazardClass       HazardClass,
    UtcTimestamp      ClassifiedAtUtc,
    PerformedByActor  Actor,
    CorrelationId     CorrelationId,
    CausationId       CausationId
) : IHazmatMaterialDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazmatMaterialClassified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClassifiedAtUtc;
}
