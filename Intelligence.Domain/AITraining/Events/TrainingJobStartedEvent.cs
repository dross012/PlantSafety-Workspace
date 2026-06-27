/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               TrainingJobStartedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.AITraining.Events;

/// <summary>Raised when a new training job is submitted.</summary>
public sealed record TrainingJobStartedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(TrainingJobStartedEvent);

    /// <summary>The model name being trained.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>The target model version.</summary>
    public string ModelVersion { get; init; } = string.Empty;
}
