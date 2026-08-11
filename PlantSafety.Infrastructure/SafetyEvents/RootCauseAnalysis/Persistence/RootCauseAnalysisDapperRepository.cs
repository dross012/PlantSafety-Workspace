// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Infrastructure
//  File:           RootCauseAnalysisDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RootCauseAnalysis read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Repositories;

namespace PlantSafety.Infrastructure.SafetyEvents.RootCauseAnalysis.Persistence;

/// <summary>
/// Dapper-backed repository for RootCauseAnalysis read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class RootCauseAnalysisDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RootCauseAnalysisDapperRepository> logger)
    : IRootCauseAnalysisRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RootCauseAnalysisItemDto>> GetRootCauseAnalysesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRootCauseAnalyses for RootCauseAnalysis tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RootCauseAnalysisItemDto>(
            "[SafetyEvents].[usp_RootCauseAnalysis_GetRootCauseAnalyses]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<RootCauseAnalysisDetailDto?> GetByIdAsync(Guid rootCauseAnalysisId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for RootCauseAnalysis {RootCauseAnalysisId} Tenant {TenantId}", rootCauseAnalysisId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RootCauseAnalysisId", rootCauseAnalysisId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<RootCauseAnalysisDetailDto>(
            "[SafetyEvents].[usp_RootCauseAnalysis_GetById]", parameters, cancellationToken);
    }
}
