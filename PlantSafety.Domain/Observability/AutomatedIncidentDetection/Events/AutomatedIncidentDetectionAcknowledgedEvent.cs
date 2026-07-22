// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionAcknowledgedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a supervisor acknowledges an automated incident detection and
//                  begins reviewing the captured video/sensor evidence.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Events;

/// <summary>Raised when an <see cref="AutomatedIncidentDetection"/> is acknowledged.</summary>
public sealed record AutomatedIncidentDetectionAcknowledgedEvent(
    AutomatedIncidentDetectionEventId EventId,
    AutomatedIncidentDetectionId      AutomatedIncidentDetectionId,
    UserId                            AcknowledgedBy,
    UtcTimestamp                      AcknowledgedAtUtc,
    PerformedByActor                  Actor,
    CorrelationId                     CorrelationId,
    CausationId                       CausationId
) : IAutomatedIncidentDetectionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutomatedIncidentDetectionAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AcknowledgedAtUtc;
}
