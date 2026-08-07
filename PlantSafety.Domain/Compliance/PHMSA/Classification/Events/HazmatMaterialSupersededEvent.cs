// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialSupersededEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a classified HazmatMaterial is superseded by a newer
//                  classification record.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Events;

/// <summary>Raised when a classified <c>HazmatMaterial</c> is superseded by a newer classification record.</summary>
public sealed record HazmatMaterialSupersededEvent(
    HazmatMaterialId HazmatMaterialId,
    HazmatMaterialId SupersededByMaterialId,
    UtcTimestamp     SupersededAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IHazmatMaterialDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazmatMaterialSuperseded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SupersededAtUtc;
}
