// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementUpdatedEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when mutable fields (Title/Description/Severity/Location/
//                  AssignedInvestigatorId) are changed on an existing IncidentManagement
//                  aggregate. Mirrors the field set actually mutated by
//                  [SafetyEvents].[usp_Incident_Update]. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

/// <summary>Raised when an existing <see cref="Aggregates.IncidentManagement"/> aggregate's mutable details are updated.</summary>
public sealed record IncidentManagementUpdatedEvent(
    IncidentManagementEventId EventId,
    IncidentManagementId      IncidentManagementId,
    UtcTimestamp      UpdatedAtUtc,
    PerformedByActor  Actor,
    CorrelationId     CorrelationId,
    CausationId       CausationId
) : IIncidentManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentManagementUpdated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => UpdatedAtUtc;
}
