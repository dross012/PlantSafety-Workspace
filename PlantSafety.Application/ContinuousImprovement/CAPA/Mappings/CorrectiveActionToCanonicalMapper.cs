// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Application
//  File:           CorrectiveActionToCanonicalMapper.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Maps the real CorrectiveAction aggregate to the enterprise canonical model
//                  (PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel
//                  .CanonicalImprovementActionV1/V2/V3) for enterprise integration consumers
//                  (ERP/MES/CMMS adapters, event mesh). Lives in this module's Application layer
//                  (not SharedKernel) because SharedKernel must never reference module
//                  Domain/Application types. V3's IsPredictedOverdue field is a deterministic
//                  projection of the same guard condition CorrectiveAction.MarkOverdue already
//                  enforces -- no genuine ML predictor exists for CAPA recurrence/overdue-risk in
//                  the Phase 5 predictor infrastructure, so nothing is fabricated. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.ContinuousImprovement.CAPA.Aggregates;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Enums;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.ContinuousImprovement.CAPA.Mappings;

/// <summary>Maps a real <see cref="CorrectiveAction"/> aggregate to the enterprise canonical improvement-action shapes.</summary>
public static class CorrectiveActionToCanonicalMapper
{
    /// <summary>Maps <paramref name="action"/> to its V1 (stable) canonical projection.</summary>
    public static CanonicalImprovementActionV1 ToCanonicalV1(this CorrectiveAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new CanonicalImprovementActionV1
        {
            ImprovementActionId = action.CorrectiveActionId.Value,
            TenantId = action.TenantId,
            SourceEventId = action.SourceEventId,
            SourceEventType = action.SourceEventType,
            Title = action.Title,
            Description = action.Description,
            ActionType = action.ActionType,
            Priority = action.Priority,
            Status = action.Status.ToString(),
            AssignedToWorkerId = action.AssignedToWorkerId.Value,
            DueAtUtc = action.DueAtUtc,
            CreatedAtUtc = action.CreatedAtUtc,
        };
    }

    /// <summary>Maps <paramref name="action"/> to its V2 (extended) canonical projection.</summary>
    public static CanonicalImprovementActionV2 ToCanonicalV2(this CorrectiveAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new CanonicalImprovementActionV2
        {
            ImprovementActionId = action.CorrectiveActionId.Value,
            TenantId = action.TenantId,
            SourceEventId = action.SourceEventId,
            SourceEventType = action.SourceEventType,
            Title = action.Title,
            Description = action.Description,
            ActionType = action.ActionType,
            Priority = action.Priority,
            Status = action.Status.ToString(),
            AssignedToWorkerId = action.AssignedToWorkerId.Value,
            DueAtUtc = action.DueAtUtc,
            CreatedAtUtc = action.CreatedAtUtc,
            CompletedAtUtc = action.CompletedAtUtc,
        };
    }

    /// <summary>
    /// Maps <paramref name="action"/> to its V3 (forward-looking) canonical projection.
    /// <paramref name="nowUtc"/> is the instant against which <see cref="CorrectiveAction.DueAtUtc"/>
    /// is compared to derive <see cref="CanonicalImprovementActionV3.IsPredictedOverdue"/> --
    /// mirroring the real <see cref="CorrectiveAction.MarkOverdue"/> guard condition, not an ML
    /// prediction.
    /// </summary>
    public static CanonicalImprovementActionV3 ToCanonicalV3(this CorrectiveAction action, UtcTimestamp nowUtc)
    {
        ArgumentNullException.ThrowIfNull(action);

        var isOpenOrInProgress = action.Status is CorrectiveActionStatus.Open or CorrectiveActionStatus.InProgress;

        return new CanonicalImprovementActionV3
        {
            ImprovementActionId = action.CorrectiveActionId.Value,
            TenantId = action.TenantId,
            SourceEventId = action.SourceEventId,
            SourceEventType = action.SourceEventType,
            Title = action.Title,
            Description = action.Description,
            ActionType = action.ActionType,
            Priority = action.Priority,
            Status = action.Status.ToString(),
            AssignedToWorkerId = action.AssignedToWorkerId.Value,
            DueAtUtc = action.DueAtUtc,
            CreatedAtUtc = action.CreatedAtUtc,
            CompletedAtUtc = action.CompletedAtUtc,
            IsPredictedOverdue = isOpenOrInProgress && nowUtc.Value > action.DueAtUtc.Value,
        };
    }
}
