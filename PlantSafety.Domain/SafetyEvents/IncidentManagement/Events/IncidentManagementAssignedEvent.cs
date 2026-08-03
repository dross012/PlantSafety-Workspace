// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementAssignedEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a technician/investigator is (re)assigned to an
//                  IncidentManagement aggregate via AssignTechnician. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

/// <summary>Raised when a technician is assigned to an <see cref="Aggregates.IncidentManagement"/> aggregate.</summary>
public sealed record IncidentManagementAssignedEvent(
    IncidentManagementEventId EventId,
    IncidentManagementId      IncidentManagementId,
    UserId                    AssignedTechnicianId,
    UtcTimestamp              AssignedAtUtc,
    PerformedByActor          Actor,
    CorrelationId             CorrelationId,
    CausationId               CausationId
) : IIncidentManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentManagementAssigned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AssignedAtUtc;
}
