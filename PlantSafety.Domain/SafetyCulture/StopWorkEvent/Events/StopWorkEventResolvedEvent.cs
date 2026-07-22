// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventResolvedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a StopWorkEvent is resolved and, where applicable, the employee
//                  is recognized per PlantSafety Program Structure "Safety Culture Module" §3
//                  ("Celebrating employees who prevent incidents") and §6 (Recognition &
//                  Reinforcement).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Events;

/// <summary>Raised when a <see cref="StopWorkEvent"/> is resolved.</summary>
public sealed record StopWorkEventResolvedEvent(
    StopWorkEventEventId EventId,
    StopWorkEventId StopWorkEventId,
    string ResolutionSummary,
    bool WasCelebrated,
    UtcTimestamp ResolvedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IStopWorkEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "StopWorkEventResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ResolvedAtUtc;
}
