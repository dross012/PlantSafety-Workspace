/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               FeaturePipelineExecutedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.FeatureEngineering.Events;

/// <summary>Raised when the feature engineering pipeline completes a run.</summary>
public sealed record FeaturePipelineExecutedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(FeaturePipelineExecutedEvent);

    /// <summary>The pipeline that executed.</summary>
    public string PipelineName { get; init; } = string.Empty;

    /// <summary>The data source that triggered the run.</summary>
    public string DataSourcePath { get; init; } = string.Empty;

    /// <summary>Number of records processed.</summary>
    public int InputRecordCount { get; init; }

    /// <summary>Number of feature sets produced.</summary>
    public int FeatureSetCount { get; init; }

    /// <summary>Execution duration in milliseconds.</summary>
    public long ExecutionDurationMs { get; init; }
}
