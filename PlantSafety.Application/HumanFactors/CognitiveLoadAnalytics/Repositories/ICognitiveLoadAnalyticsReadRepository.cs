// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           ICognitiveLoadAnalyticsReadRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the HumanFactors.CognitiveLoadAnalytics
//                  read-model dashboards (Cognitive Load by Shift/Role, Task Complexity vs. Load
//                  Correlation, Predictive Cognitive Risk Board). Implemented in the Infrastructure
//                  layer. Distinct from the Domain-layer write-side
//                  ICognitiveLoadLedgerRepository/ICognitiveRiskPredictionRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

/// <summary>Read-only repository for HumanFactors.CognitiveLoadAnalytics dashboard data.</summary>
public interface ICognitiveLoadAnalyticsReadRepository
{
    /// <summary>Returns a single worker's cognitive-load rollup for the given period, or <see langword="null"/> if no readings exist.</summary>
    Task<CognitiveLoadByWorkerDto?> GetCognitiveLoadByWorkerAsync(
        Guid tenantId, Guid workerUserId, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken);

    /// <summary>Returns a single task-complexity profile by task code, or <see langword="null"/> if not defined.</summary>
    Task<TaskComplexityProfileDto?> GetTaskComplexityProfileAsync(
        Guid tenantId, string taskCode, CancellationToken cancellationToken);

    /// <summary>Returns the workload-trend snapshots for the given tenant/site/shift within a period.</summary>
    Task<IReadOnlyList<WorkloadTrendByShiftDto>> GetWorkloadTrendByShiftAsync(
        Guid tenantId, Guid? siteId, string shiftType, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken);

    /// <summary>Returns all Elevated/Critical, non-dismissed cognitive-risk predictions for the given tenant/site.</summary>
    Task<IReadOnlyList<CognitiveRiskPredictionDto>> ListElevatedCognitiveRiskPredictionsAsync(
        Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}
