// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new EmergencyDrillPlanning aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Events;

/// <summary>Raised when a new <see cref="EmergencyDrillPlanning"/> aggregate is created.</summary>
public sealed record EmergencyDrillPlanningCreatedEvent(
    EmergencyDrillPlanningEventId EventId,
    EmergencyDrillPlanningId      EmergencyDrillPlanningId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IEmergencyDrillPlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyDrillPlanningCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}