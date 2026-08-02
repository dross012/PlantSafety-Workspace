// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Domain
//  File:           DataLakeActivityLifecycleEvents.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events for the DataLakeActivity aggregate's full lifecycle
//                  (Create/StartProcessing/Curate/Fail/Archive). Phase 4 full build-out -- this
//                  aggregate did not exist before this pass (only a placeholder marker class and a
//                  read-only date-range query slice existed). Mirrors
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Events.RCAEvents.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.DataLake.Events;

/// <summary>Raised when a new Intelligence.DataLake activity record is created.</summary>
public sealed record DataLakeActivityCreatedEvent(
    DataLakeActivityEventId EventId,
    DataLakeActivityId DataLakeActivityId,
    Guid TenantId,
    Guid? SiteId,
    string DatasetName,
    string Zone,
    long? RecordCount,
    UserId ProcessedBy,
    UtcTimestamp ProcessedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDataLakeActivityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DataLakeActivityCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ProcessedAtUtc;
}

/// <summary>Raised when processing begins on a data lake activity (Ingested -&gt; Processing).</summary>
public sealed record DataLakeActivityProcessingStartedEvent(
    DataLakeActivityEventId EventId,
    DataLakeActivityId DataLakeActivityId,
    UserId StartedBy,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDataLakeActivityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DataLakeActivityProcessingStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when a data lake activity's data has been curated (Processing -&gt; Curated).</summary>
public sealed record DataLakeActivityCuratedEvent(
    DataLakeActivityEventId EventId,
    DataLakeActivityId DataLakeActivityId,
    long? FinalRecordCount,
    UtcTimestamp CuratedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDataLakeActivityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DataLakeActivityCurated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CuratedAtUtc;
}

/// <summary>Raised when a data lake activity fails (Processing -&gt; Failed).</summary>
public sealed record DataLakeActivityFailedEvent(
    DataLakeActivityEventId EventId,
    DataLakeActivityId DataLakeActivityId,
    string FailureReason,
    UtcTimestamp FailedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDataLakeActivityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DataLakeActivityFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FailedAtUtc;
}

/// <summary>Raised when a curated or failed data lake activity is archived for compliance retention (Curated/Failed -&gt; Archived).</summary>
public sealed record DataLakeActivityArchivedEvent(
    DataLakeActivityEventId EventId,
    DataLakeActivityId DataLakeActivityId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDataLakeActivityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DataLakeActivityArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
