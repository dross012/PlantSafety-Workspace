// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new PPETrainingFit aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Events;

/// <summary>Raised when a new <see cref="PPETrainingFit"/> aggregate is created.</summary>
public sealed record PPETrainingFitCreatedEvent(
    PPETrainingFitEventId EventId,
    PPETrainingFitId      PPETrainingFitId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IPPETrainingFitDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPETrainingFitCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}