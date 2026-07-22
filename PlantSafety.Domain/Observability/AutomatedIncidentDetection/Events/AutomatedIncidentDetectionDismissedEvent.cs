// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionDismissedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when an automated incident detection is reviewed and dismissed as a
//                  false positive.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Events;

/// <summary>Raised when an <see cref="AutomatedIncidentDetection"/> is dismissed as a false positive.</summary>
public sealed record AutomatedIncidentDetectionDismissedEvent(
    AutomatedIncidentDetectionEventId EventId,
    AutomatedIncidentDetectionId      AutomatedIncidentDetectionId,
    string                            Reason,
    UtcTimestamp                      DismissedAtUtc,
    PerformedByActor                  Actor,
    CorrelationId                     CorrelationId,
    CausationId                       CausationId
) : IAutomatedIncidentDetectionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutomatedIncidentDetectionDismissed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DismissedAtUtc;
}
