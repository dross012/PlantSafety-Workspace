// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementArchivedEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when an IncidentManagement aggregate is archived (soft-deleted).
//                  Mirrors the established Archive-event convention used by AccidentManagement
//                  (AccidentArchivedEvent) and RiskManagement (RiskArchivedEvent), and the real
//                  transition performed by [SafetyEvents].[usp_Incident_SoftDelete] (Status ->
//                  N'Archived', IsDeleted -> 1). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

/// <summary>Raised when an <see cref="Aggregates.IncidentManagement"/> aggregate is archived (soft-deleted) for long-term retention.</summary>
public sealed record IncidentManagementArchivedEvent(
    IncidentManagementEventId EventId,
    IncidentManagementId      IncidentManagementId,
    UtcTimestamp      ArchivedAtUtc,
    string             DeletionReason,
    PerformedByActor  Actor,
    CorrelationId     CorrelationId,
    CausationId       CausationId
) : IIncidentManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentManagementArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
