// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationCreatedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new LocalEmergencyCoordination aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Events;

/// <summary>Raised when a new <see cref="Aggregates.LocalEmergencyCoordination"/> aggregate is created.</summary>
public sealed record LocalEmergencyCoordinationCreatedEvent(
    LocalEmergencyCoordinationEventId EventId,
    LocalEmergencyCoordinationId      LocalEmergencyCoordinationId,
    UtcTimestamp      CreatedAtUtc,
    PerformedByActor  Actor,
    CorrelationId     CorrelationId,
    CausationId       CausationId
) : ILocalEmergencyCoordinationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LocalEmergencyCoordinationCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
