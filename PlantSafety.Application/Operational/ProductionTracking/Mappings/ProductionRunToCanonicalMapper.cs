// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           ProductionRunToCanonicalMapper.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Maps the real ProductionRun aggregate to the enterprise canonical model
//                  (PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel
//                  .CanonicalProductionOrderV1/V2/V3) for enterprise integration consumers
//                  (ERP/MES/CMMS adapters, event mesh). Lives in this module's Application layer
//                  (not SharedKernel) because SharedKernel must never reference module
//                  Domain/Application types. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Aggregates;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Operational.ProductionTracking.Mappings;

/// <summary>Maps a real <see cref="ProductionRun"/> aggregate to the enterprise canonical production-order shapes.</summary>
public static class ProductionRunToCanonicalMapper
{
    /// <summary>Maps <paramref name="run"/> to its V1 (stable) canonical projection.</summary>
    public static CanonicalProductionOrderV1 ToCanonicalV1(this ProductionRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        return new CanonicalProductionOrderV1
        {
            ProductionOrderId = run.ProductionRunId.Value,
            TenantId = run.TenantId,
            SiteId = run.SiteId,
            OrderCode = run.RunCode,
            ProductionLineId = run.LineId.Value,
            ProductionLineName = run.LineName,
            AssetId = run.MachineId,
            Status = run.Status.ToString(),
            StartedAtUtc = run.StartedAtUtc,
            CreatedAtUtc = run.CreatedAtUtc,
            UpdatedAtUtc = run.UpdatedAtUtc,
        };
    }

    /// <summary>Maps <paramref name="run"/> to its V2 (extended) canonical projection.</summary>
    public static CanonicalProductionOrderV2 ToCanonicalV2(this ProductionRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        return new CanonicalProductionOrderV2
        {
            ProductionOrderId = run.ProductionRunId.Value,
            TenantId = run.TenantId,
            SiteId = run.SiteId,
            OrderCode = run.RunCode,
            ProductionLineId = run.LineId.Value,
            ProductionLineName = run.LineName,
            AssetId = run.MachineId,
            Status = run.Status.ToString(),
            StartedAtUtc = run.StartedAtUtc,
            CreatedAtUtc = run.CreatedAtUtc,
            UpdatedAtUtc = run.UpdatedAtUtc,
            OperatorName = run.OperatorName,
            IdealCycleTimeSeconds = run.IdealCycleTimeSeconds,
            GoodUnitCount = run.GoodUnitCount,
            ScrapUnitCount = run.ScrapUnitCount,
            DowntimeMinutesTotal = run.DowntimeMinutesTotal,
            EndedAtUtc = run.EndedAtUtc,
        };
    }

    /// <summary>
    /// Maps <paramref name="run"/> to its V3 (predictive) canonical projection. No dedicated ML
    /// predictor exists for production-order completion; predicted-completion fields are populated
    /// only when the caller genuinely ran
    /// <see cref="PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors.TaskDurationPredictor"/>
    /// against this run's historical cycle-time data. Callers with no genuine prediction should
    /// pass nulls rather than fabricate values.
    /// </summary>
    public static CanonicalProductionOrderV3 ToCanonicalV3(
        this ProductionRun run,
        UtcTimestamp? predictedCompletionAtUtc = null,
        double? predictedCompletionConfidence = null,
        string? predictionModelVersion = null,
        UtcTimestamp? predictionGeneratedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(run);

        return new CanonicalProductionOrderV3
        {
            ProductionOrderId = run.ProductionRunId.Value,
            TenantId = run.TenantId,
            SiteId = run.SiteId,
            OrderCode = run.RunCode,
            ProductionLineId = run.LineId.Value,
            ProductionLineName = run.LineName,
            AssetId = run.MachineId,
            Status = run.Status.ToString(),
            StartedAtUtc = run.StartedAtUtc,
            CreatedAtUtc = run.CreatedAtUtc,
            UpdatedAtUtc = run.UpdatedAtUtc,
            OperatorName = run.OperatorName,
            IdealCycleTimeSeconds = run.IdealCycleTimeSeconds,
            GoodUnitCount = run.GoodUnitCount,
            ScrapUnitCount = run.ScrapUnitCount,
            DowntimeMinutesTotal = run.DowntimeMinutesTotal,
            EndedAtUtc = run.EndedAtUtc,
            PredictedCompletionAtUtc = predictedCompletionAtUtc,
            PredictedCompletionConfidence = predictedCompletionConfidence,
            PredictionModelVersion = predictionModelVersion,
            PredictionGeneratedAtUtc = predictionGeneratedAtUtc,
        };
    }
}
