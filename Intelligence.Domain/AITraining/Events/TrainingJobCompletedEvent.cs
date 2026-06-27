/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               TrainingJobCompletedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.AITraining.Events;

/// <summary>Raised when a training job completes successfully.</summary>
public sealed record TrainingJobCompletedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(TrainingJobCompletedEvent);

    /// <summary>The model name that was trained.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>The model version produced.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>The primary accuracy score.</summary>
    public double AccuracyScore { get; init; }

    /// <summary>The name of the accuracy metric (e.g., "F1").</summary>
    public string MetricName { get; init; } = string.Empty;

    /// <summary>The SHA-256 hash of the training dataset used.</summary>
    public string DatasetHash { get; init; } = string.Empty;

    /// <summary>Whether the accuracy score meets the minimum deployment threshold.</summary>
    public bool MeetsDeploymentThreshold { get; init; }
}
