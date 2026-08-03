// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           OptimizationRunToCanonicalMapper.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Maps the real OptimizationRun aggregate's committed ScheduleAssignment value
//                  objects to the enterprise canonical model
//                  (PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel
//                  .CanonicalSchedulingBlockV1/V2/V3) for enterprise integration consumers
//                  (ERP/MES/CMMS adapters, event mesh). One CanonicalSchedulingBlock is produced
//                  per ScheduleAssignment in OptimizationRun.ResultAssignments, since a raw
//                  ScheduleAssignment carries no identity of its own -- SchedulingBlockId is
//                  deterministically derived (Guid v5-style deterministic hash) from
//                  (OptimizationRunId, TaskId, ResourceId) so repeated mapping of the same run
//                  produces stable identifiers. Lives in this module's Application layer (not
//                  SharedKernel) because SharedKernel must never reference module
//                  Domain/Application types. Additive-only evolution.
// ============================================================================
using System.Security.Cryptography;
using System.Text;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Mappings;

/// <summary>Maps a real <see cref="OptimizationRun"/> aggregate's committed assignments to the enterprise canonical scheduling-block shapes.</summary>
public static class OptimizationRunToCanonicalMapper
{
    /// <summary>
    /// Deterministically derives a stable <see cref="Guid"/> identity for a scheduling block from
    /// its owning run and the task/resource it places, since <see cref="ScheduleAssignment"/>
    /// carries no identity of its own.
    /// </summary>
    public static Guid DeriveSchedulingBlockId(Guid optimizationRunId, Guid taskId, Guid resourceId)
    {
        var input = $"{optimizationRunId:N}:{taskId:N}:{resourceId:N}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash[..16]);
    }

    /// <summary>Maps every committed assignment on <paramref name="run"/> to its V1 (stable) canonical projection.</summary>
    public static IReadOnlyList<CanonicalSchedulingBlockV1> ToCanonicalV1(this OptimizationRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        return run.ResultAssignments
            .Select(assignment => new CanonicalSchedulingBlockV1
            {
                SchedulingBlockId = DeriveSchedulingBlockId(run.OptimizationRunId.Value, assignment.TaskId.Value, assignment.ResourceId.Value),
                TenantId = run.TenantId,
                SiteId = run.SiteId,
                OptimizationRunId = run.OptimizationRunId.Value,
                TaskId = assignment.TaskId.Value,
                ResourceId = assignment.ResourceId.Value,
                Status = run.Status.ToString(),
                StartUtc = assignment.StartUtc,
                EndUtc = assignment.EndUtc,
                CreatedAtUtc = run.RequestedAtUtc,
            })
            .ToList();
    }

    /// <summary>Maps every committed assignment on <paramref name="run"/> to its V2 (extended) canonical projection.</summary>
    public static IReadOnlyList<CanonicalSchedulingBlockV2> ToCanonicalV2(this OptimizationRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        return run.ResultAssignments
            .Select(assignment => new CanonicalSchedulingBlockV2
            {
                SchedulingBlockId = DeriveSchedulingBlockId(run.OptimizationRunId.Value, assignment.TaskId.Value, assignment.ResourceId.Value),
                TenantId = run.TenantId,
                SiteId = run.SiteId,
                OptimizationRunId = run.OptimizationRunId.Value,
                TaskId = assignment.TaskId.Value,
                ResourceId = assignment.ResourceId.Value,
                Status = run.Status.ToString(),
                StartUtc = assignment.StartUtc,
                EndUtc = assignment.EndUtc,
                CreatedAtUtc = run.RequestedAtUtc,
                EffectivePriorityWeight = assignment.EffectivePriorityWeight,
                Objective = run.Objective.ToString(),
                SelectedStrategyName = run.SelectedStrategyName,
                RunScore = run.Score,
            })
            .ToList();
    }

    /// <summary>
    /// Maps a single committed <paramref name="assignment"/> on <paramref name="run"/> to its V3
    /// (predictive) canonical projection. Predictive fields are populated only when the caller
    /// genuinely ran the corresponding Analytics.PredictiveAnalytics.Predictors model for this
    /// assignment; callers with no genuine prediction should pass nulls rather than fabricate
    /// values.
    /// </summary>
    public static CanonicalSchedulingBlockV3 ToCanonicalV3(
        this OptimizationRun run,
        ScheduleAssignment assignment,
        double? predictedConflictLikelihood = null,
        TimeSpan? predictedDuration = null,
        double? recommendedWindowScore = null,
        string? predictionModelVersion = null,
        UtcTimestamp? predictionGeneratedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(assignment);

        return new CanonicalSchedulingBlockV3
        {
            SchedulingBlockId = DeriveSchedulingBlockId(run.OptimizationRunId.Value, assignment.TaskId.Value, assignment.ResourceId.Value),
            TenantId = run.TenantId,
            SiteId = run.SiteId,
            OptimizationRunId = run.OptimizationRunId.Value,
            TaskId = assignment.TaskId.Value,
            ResourceId = assignment.ResourceId.Value,
            Status = run.Status.ToString(),
            StartUtc = assignment.StartUtc,
            EndUtc = assignment.EndUtc,
            CreatedAtUtc = run.RequestedAtUtc,
            EffectivePriorityWeight = assignment.EffectivePriorityWeight,
            Objective = run.Objective.ToString(),
            SelectedStrategyName = run.SelectedStrategyName,
            RunScore = run.Score,
            PredictedConflictLikelihood = predictedConflictLikelihood,
            PredictedDuration = predictedDuration,
            RecommendedWindowScore = recommendedWindowScore,
            PredictionModelVersion = predictionModelVersion,
            PredictionGeneratedAtUtc = predictionGeneratedAtUtc,
        };
    }
}
