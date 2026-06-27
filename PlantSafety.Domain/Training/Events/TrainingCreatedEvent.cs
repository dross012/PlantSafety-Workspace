// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Training
//  Layer:          Domain
//  File:           TrainingCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a Training aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Training.Events;

/// <summary>Raised when a new <see cref="Training"/> aggregate is created.</summary>
public sealed record TrainingCreatedEvent(
    Guid          EventId,
    Guid          TrainingId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : ITrainingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TrainingCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
