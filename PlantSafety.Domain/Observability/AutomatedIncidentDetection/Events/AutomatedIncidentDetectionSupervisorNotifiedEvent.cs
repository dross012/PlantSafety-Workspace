// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionSupervisorNotifiedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when the assigned supervisor is instantly notified of an automated
//                  incident detection ("Instant notifications to supervisors").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Events;

/// <summary>Raised when a supervisor is notified of an <see cref="AutomatedIncidentDetection"/>.</summary>
public sealed record AutomatedIncidentDetectionSupervisorNotifiedEvent(
    AutomatedIncidentDetectionEventId EventId,
    AutomatedIncidentDetectionId      AutomatedIncidentDetectionId,
    UserId                            NotifiedSupervisorUserId,
    UtcTimestamp                      NotifiedAtUtc,
    PerformedByActor                  Actor,
    CorrelationId                     CorrelationId,
    CausationId                       CausationId
) : IAutomatedIncidentDetectionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutomatedIncidentDetectionSupervisorNotified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => NotifiedAtUtc;
}
