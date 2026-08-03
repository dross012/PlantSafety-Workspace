// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunEvents.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised by the OptimizationRun aggregate for its lifecycle
//                  transitions (started/completed/failed). Every event carries the canonical
//                  causality/correlation envelope (Handbook §2.17-2.18).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.DomainEvents;

/// <summary>Raised when an OptimizationRun transitions from Pending to Running (the optimization pipeline begins executing).</summary>
public sealed record OptimizationRunStartedEvent(
    OptimizationRunEventId EventId,
    OptimizationRunId OptimizationRunId,
    OptimizationObjective Objective,
    int TaskCount,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOptimizationRunDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OptimizationRunStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when an OptimizationRun completes successfully and its selected result is recorded.</summary>
public sealed record OptimizationRunCompletedEvent(
    OptimizationRunEventId EventId,
    OptimizationRunId OptimizationRunId,
    string SelectedStrategyName,
    int AssignmentCount,
    int UnscheduledTaskCount,
    int ToleratedViolationCount,
    double Score,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOptimizationRunDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OptimizationRunCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when an OptimizationRun could not produce a result (e.g. an unhandled exception during strategy execution).</summary>
public sealed record OptimizationRunFailedEvent(
    OptimizationRunEventId EventId,
    OptimizationRunId OptimizationRunId,
    string Reason,
    UtcTimestamp FailedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOptimizationRunDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OptimizationRunFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FailedAtUtc;
}
