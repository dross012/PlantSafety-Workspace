// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventInvestigatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a Just Culture (non-punitive) investigation of a StopWorkEvent
//                  is recorded, distinguishing human error from negligence and identifying
//                  root cause per PlantSafety Program Structure "Safety Culture Module" §2.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Events;

/// <summary>Raised when a <see cref="StopWorkEvent"/> investigation is recorded.</summary>
public sealed record StopWorkEventInvestigatedEvent(
    StopWorkEventEventId EventId,
    StopWorkEventId StopWorkEventId,
    string Findings,
    UtcTimestamp InvestigatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IStopWorkEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "StopWorkEventInvestigated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => InvestigatedAtUtc;
}
