// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerMarkedEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when text markings and hazard labels are applied to a HazmatContainer.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Events;

/// <summary>Raised when markings and labels are applied (Draft -&gt; MarkedAndLabeled).</summary>
public sealed record HazmatContainerMarkedEvent(
    HazmatContainerEventId EventId,
    HazmatContainerId HazmatContainerId,
    IReadOnlyList<string> Markings,
    IReadOnlyList<string> Labels,
    bool OrientationArrowsRequired,
    string? CompatibilityGroup,
    UtcTimestamp MarkedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazmatContainerDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazmatContainerMarked";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => MarkedAtUtc;
}
