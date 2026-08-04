// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Infrastructure
//  File:           CognitiveLoadAnalyticsReadDapperRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed repository for the HumanFactors.CognitiveLoadAnalytics
//                  Application-layer read-model. Backs the Cognitive Load by Shift/Role, Task
//                  Complexity vs. Load Correlation, and Predictive Cognitive Risk Board
//                  dashboards. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

namespace PlantSafety.Infrastructure.HumanFactors.CognitiveLoadAnalytics.Persistence;

/// <summary>Dapper-backed repository for HumanFactors.CognitiveLoadAnalytics dashboard read-model queries.</summary>
public sealed class CognitiveLoadAnalyticsReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<CognitiveLoadAnalyticsReadDapperRepository> logger)
    : ICognitiveLoadAnalyticsReadRepository
{
    /// <inheritdoc/>
    public async Task<CognitiveLoadByWorkerDto?> GetCognitiveLoadByWorkerAsync(
        Guid tenantId, Guid workerUserId, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("WorkerUserId", workerUserId)
            .AddDateTime("PeriodStartUtc", periodStartUtc)
            .AddDateTime("PeriodEndUtc", periodEndUtc)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CognitiveLoadByWorkerDto>(
            "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadAnalytics_GetCognitiveLoadByWorker]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TaskComplexityProfileDto?> GetTaskComplexityProfileAsync(Guid tenantId, string taskCode, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddString("TaskCode", taskCode, 50).Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<TaskComplexityProfileDto>(
            "[HumanFactorsCognitiveLoadAnalytics].[usp_TaskComplexityProfile_GetByCode]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkloadTrendByShiftDto>> GetWorkloadTrendByShiftAsync(
        Guid tenantId, Guid? siteId, string shiftType, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ShiftType", shiftType, 20)
            .AddDateTime("PeriodStartUtc", periodStartUtc)
            .AddDateTime("PeriodEndUtc", periodEndUtc)
            .Build();

        return await queryExecutor.QueryAsync<WorkloadTrendByShiftDto>(
            "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadAnalytics_GetWorkloadTrendByShift]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CognitiveRiskPredictionDto>> ListElevatedCognitiveRiskPredictionsAsync(
        Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("SiteId", siteId).Build();

        return await queryExecutor.QueryAsync<CognitiveRiskPredictionDto>(
            "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_ListElevatedByTenant]", parameters, cancellationToken);
    }
}
