/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ModelRegisteredEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.ModelRegistry.Events;

/// <summary>Raised when a new model version is registered in the model registry.</summary>
public sealed record ModelRegisteredEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(ModelRegisteredEvent);

    /// <summary>The logical model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>The semantic version of the registered model.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>The training dataset hash for lineage tracking.</summary>
    public string DatasetHash { get; init; } = string.Empty;

    /// <summary>The accuracy score recorded at training completion.</summary>
    public double TrainingAccuracyScore { get; init; }
}
