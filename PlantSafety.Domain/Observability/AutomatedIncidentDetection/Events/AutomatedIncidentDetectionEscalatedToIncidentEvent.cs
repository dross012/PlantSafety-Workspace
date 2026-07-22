// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionEscalatedToIncidentEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a confirmed automated incident detection is escalated into a
//                  formal Incident Management record.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Events;

/// <summary>Raised when an <see cref="AutomatedIncidentDetection"/> is escalated into an incident record.</summary>
public sealed record AutomatedIncidentDetectionEscalatedToIncidentEvent(
    AutomatedIncidentDetectionEventId EventId,
    AutomatedIncidentDetectionId      AutomatedIncidentDetectionId,
    Guid                              IncidentReferenceId,
    UtcTimestamp                      EscalatedAtUtc,
    PerformedByActor                  Actor,
    CorrelationId                     CorrelationId,
    CausationId                       CausationId
) : IAutomatedIncidentDetectionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutomatedIncidentDetectionEscalatedToIncident";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EscalatedAtUtc;
}
