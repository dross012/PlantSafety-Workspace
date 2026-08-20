// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureEngineeringEvents.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Intelligence.FeatureEngineering bounded context. Ported
//                  (rebuilt) from the orphaned, dead
//                  Intelligence.Domain/FeatureEngineering/Events/{FeatureSetPublishedEvent,
//                  FeaturePipelineExecutedEvent}.cs records, folding in EventId/Actor/Correlation/
//                  Causation envelope fields per this module's own convention. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Events.DataLakeActivityLifecycleEvents.
//                  FeatureSetPublishedEvent triggers downstream training preparation (consumed by
//                  Intelligence.AITraining).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.FeatureEngineering.Events;

/// <summary>Raised when a feature set is created (unpublished) in the feature store.</summary>
public sealed record FeatureSetCreatedEvent(
    FeatureSetEventId EventId,
    FeatureSetId FeatureSetId,
    Guid TenantId,
    Guid? SiteId,
    string Name,
    string Version,
    string DataSourcePath,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFeatureEngineeringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FeatureSetCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc { get; init; } = UtcTimestamp.Now();
}

/// <summary>Raised when a feature set is published to the feature store. Triggers downstream training preparation. Immutable thereafter.</summary>
public sealed record FeatureSetPublishedEvent(
    FeatureSetEventId EventId,
    FeatureSetId FeatureSetId,
    string Name,
    string Version,
    int FeatureCount,
    UtcTimestamp PublishedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFeatureEngineeringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FeatureSetPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}

/// <summary>Raised when the feature engineering pipeline completes a run over a data source, ahead of any resulting FeatureSet publication.</summary>
public sealed record FeaturePipelineExecutedEvent(
    FeatureSetEventId EventId,
    Guid TenantId,
    string PipelineName,
    string DataSourcePath,
    int InputRecordCount,
    int FeatureSetCount,
    long ExecutionDurationMs,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFeatureEngineeringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FeaturePipelineExecuted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc { get; init; } = UtcTimestamp.Now();
}
