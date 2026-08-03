// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.CanonicalModel
//  Layer:          SharedKernel
//  File:           CanonicalSchedulingBlock.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Canonical, enterprise-stable projection of a single scheduled placement (a
//                  "block") produced by the predictive scheduling optimization pipeline. Grounded
//                  in PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates.OptimizationRun
//                  and its ScheduleAssignment value objects -- one CanonicalSchedulingBlock per
//                  committed ScheduleAssignment, carrying enough OptimizationRun-level context
//                  (objective, strategy, score) for enterprise consumers without requiring them to
//                  understand the full optimization-run shape. Not a new parallel domain state.
//                  See README.md in this folder for the versioning convention and backward-
//                  compatibility rules. Additive-only evolution: CanonicalSchedulingBlockV2 only
//                  adds to CanonicalSchedulingBlockV1; CanonicalSchedulingBlockV3 only adds to
//                  CanonicalSchedulingBlockV2.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

/// <summary>
/// V1 (stable) canonical projection of a scheduled block: identity, placement window, and the
/// owning optimization run's identity/status. The minimal shape external integration adapters
/// should target first.
/// </summary>
public sealed record CanonicalSchedulingBlockV1
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 1;

    /// <summary>
    /// Stable enterprise identifier of this scheduling block. Deterministically derived by the
    /// owning mapper from (OptimizationRunId, TaskId, ResourceId) since a raw ScheduleAssignment
    /// value object carries no identity of its own.
    /// </summary>
    public required Guid SchedulingBlockId { get; init; }

    /// <summary>Tenant that owns this scheduling block.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this scheduling block is scoped to, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the owning OptimizationRun that produced this block.</summary>
    public required Guid OptimizationRunId { get; init; }

    /// <summary>Identity of the task placed by this block (mirrors the source SchedulableTaskId).</summary>
    public required Guid TaskId { get; init; }

    /// <summary>Identity of the resource this block is placed against (mirrors the source SchedulingResourceId).</summary>
    public required Guid ResourceId { get; init; }

    /// <summary>Normalized status of the owning optimization run (Pending/Running/Completed/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant the task is planned to start.</summary>
    public required UtcTimestamp StartUtc { get; init; }

    /// <summary>UTC instant the task is planned to finish.</summary>
    public required UtcTimestamp EndUtc { get; init; }

    /// <summary>UTC instant the owning optimization run was requested.</summary>
    public required UtcTimestamp CreatedAtUtc { get; init; }
}

/// <summary>
/// V2 (extended) canonical projection of a scheduled block. Additive-only over
/// <see cref="CanonicalSchedulingBlockV1"/>: adds optimization-run detail already tracked by the
/// real OptimizationRun aggregate.
/// </summary>
public sealed record CanonicalSchedulingBlockV2
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 2;

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.SchedulingBlockId"/>
    public required Guid SchedulingBlockId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.OptimizationRunId"/>
    public required Guid OptimizationRunId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.TaskId"/>
    public required Guid TaskId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.ResourceId"/>
    public required Guid ResourceId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.StartUtc"/>
    public required UtcTimestamp StartUtc { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.EndUtc"/>
    public required UtcTimestamp EndUtc { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Composite priority weight used to order this block relative to others in the same run.</summary>
    public double? EffectivePriorityWeight { get; init; }

    /// <summary>Objective the owning optimization run was requested to optimize for.</summary>
    public string? Objective { get; init; }

    /// <summary>Name of the strategy selected as best-scoring for the owning run, once completed.</summary>
    public string? SelectedStrategyName { get; init; }

    /// <summary>Composite desirability score of the owning run's selected result, once completed.</summary>
    public double? RunScore { get; init; }
}

/// <summary>
/// V3 (predictive) canonical projection of a scheduled block. Additive-only over
/// <see cref="CanonicalSchedulingBlockV2"/>: adds forward-looking output wired to the real Phase 5
/// scheduling predictors (<see cref="PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors.OptimalSchedulingWindowPredictor"/>,
/// <see cref="PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors.ResourceConflictPredictor"/>,
/// <see cref="PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors.TaskDurationPredictor"/>).
/// Populated only when the owning module's mapper has genuine input features for the relevant
/// predictor; otherwise left null -- never fabricated.
/// </summary>
public sealed record CanonicalSchedulingBlockV3
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 3;

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.SchedulingBlockId"/>
    public required Guid SchedulingBlockId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.OptimizationRunId"/>
    public required Guid OptimizationRunId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.TaskId"/>
    public required Guid TaskId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.ResourceId"/>
    public required Guid ResourceId { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.StartUtc"/>
    public required UtcTimestamp StartUtc { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.EndUtc"/>
    public required UtcTimestamp EndUtc { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV2.EffectivePriorityWeight"/>
    public double? EffectivePriorityWeight { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV2.Objective"/>
    public string? Objective { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV2.SelectedStrategyName"/>
    public string? SelectedStrategyName { get; init; }

    /// <inheritdoc cref="CanonicalSchedulingBlockV2.RunScore"/>
    public double? RunScore { get; init; }

    /// <summary>
    /// Estimated likelihood (0.0-1.0) that this block's window/resource pairing produces a
    /// resource conflict, from <see cref="PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors.ResourceConflictPredictor"/>.
    /// </summary>
    public double? PredictedConflictLikelihood { get; init; }

    /// <summary>
    /// Predicted task duration, from <see cref="PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors.TaskDurationPredictor"/>.
    /// </summary>
    public TimeSpan? PredictedDuration { get; init; }

    /// <summary>
    /// Composite desirability score of this window, from
    /// <see cref="PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors.OptimalSchedulingWindowPredictor"/>.
    /// </summary>
    public double? RecommendedWindowScore { get; init; }

    /// <summary>Traceable model name/version identifier for the prediction(s) above.</summary>
    public string? PredictionModelVersion { get; init; }

    /// <summary>UTC instant the prediction(s) were generated.</summary>
    public UtcTimestamp? PredictionGeneratedAtUtc { get; init; }
}
