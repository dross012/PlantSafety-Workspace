// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardCompilationEvents.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the real scorecard compilation/distribution
//                  lifecycle: KPI measurement recording, publication, distribution, closure.
//                  Grounded in the Safety Scorecards Module of
//                  PlantSafety_Program_Structure_extract.txt.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scorecards.Enums;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;

namespace PlantSafety.Domain.Scorecards.Events;

/// <summary>Raised when a KPI measurement is recorded/updated for a scorecard's reporting period.</summary>
public sealed record ScorecardMetricRecordedEvent(
    ScorecardEventId  EventId,
    ScorecardId       ScorecardId,
    KpiMeasurementId  KpiMeasurementId,
    KpiMetricDefinition Metric,
    decimal           ComputedValue,
    UtcTimestamp      RecordedAtUtc,
    PerformedByActor  Actor,
    CorrelationId     CorrelationId,
    CausationId       CausationId
) : IScorecardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScorecardMetricRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}

/// <summary>Raised when a scorecard's KPI compilation is complete for the period.</summary>
public sealed record ScorecardCompiledEvent(
    ScorecardEventId EventId,
    ScorecardId      ScorecardId,
    int              MetricCount,
    UtcTimestamp     CompiledAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IScorecardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScorecardCompiled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompiledAtUtc;
}

/// <summary>Raised when a Compiled scorecard is published (Reporting & Distribution).</summary>
public sealed record ScorecardPublishedEvent(
    ScorecardEventId EventId,
    ScorecardId      ScorecardId,
    UtcTimestamp     PublishedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IScorecardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScorecardPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}

/// <summary>Raised when a published scorecard is distributed through a channel (dashboard, leadership review, OSHA inspection, posted area, monthly meeting, annual report).</summary>
public sealed record ScorecardDistributedEvent(
    ScorecardEventId EventId,
    ScorecardId      ScorecardId,
    string           Channel,
    UtcTimestamp     DistributedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IScorecardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScorecardDistributed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DistributedAtUtc;
}

/// <summary>Raised when a scorecard's reporting period ends and the scorecard is closed.</summary>
public sealed record ScorecardClosedEvent(
    ScorecardEventId EventId,
    ScorecardId      ScorecardId,
    UtcTimestamp     ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IScorecardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScorecardClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
