// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Predictive.FatigueRiskModeling
//  Layer:          Infrastructure
//  File:           FatigueRiskModelingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the FatigueRiskModeling read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Models;
using PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Predictive.FatigueRiskModeling.Persistence;

/// <summary>Dapper-backed repository for <see cref="FatigueRiskModelingItemDto"/> queries.</summary>
public sealed class FatigueRiskModelingDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<FatigueRiskModelingDapperRepository> logger)
    : IFatigueRiskModelingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<FatigueRiskModelingItemDto>> GetFatigueRiskAssessmentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetFatigueRiskAssessmentsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<FatigueRiskModelingItemDto>(
            "[Analytics].[usp_FatigueRiskModeling_List]", parameters, cancellationToken);
    }
}
