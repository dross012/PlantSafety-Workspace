// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.JobSafetyAnalysis
//  Layer:          Infrastructure
//  File:           JobSafetyAnalysisDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the JobSafetyAnalysis read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.JobSafetyAnalysis.Models;
using PlantSafety.Application.Operational.JobSafetyAnalysis.Repositories;

namespace PlantSafety.Infrastructure.Operational.JobSafetyAnalysis.Persistence;

/// <summary>Dapper-backed repository for <see cref="JobSafetyAnalysisItemDto"/> queries.</summary>
public sealed class JobSafetyAnalysisDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<JobSafetyAnalysisDapperRepository> logger)
    : IJobSafetyAnalysisRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<JobSafetyAnalysisItemDto>> GetJobSafetyAnalysisAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetJobSafetyAnalysisAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<JobSafetyAnalysisItemDto>(
            "[Operational].[usp_JobSafetyAnalysis_List]", parameters, cancellationToken);
    }
}
