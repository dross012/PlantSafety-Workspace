// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new AutomatedIncidentDetection aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Events;

/// <summary>Raised when a new <see cref="AutomatedIncidentDetection"/> aggregate is created.</summary>
public sealed record AutomatedIncidentDetectionCreatedEvent(
    AutomatedIncidentDetectionEventId EventId,
    AutomatedIncidentDetectionId      AutomatedIncidentDetectionId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IAutomatedIncidentDetectionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutomatedIncidentDetectionCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}