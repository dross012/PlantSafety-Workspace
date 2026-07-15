// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange.RiskAssessments
//  Layer:          Infrastructure
//  File:           RiskAssessmentsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ManagementOfChange.RiskAssessments read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Models;
using PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Repositories;

namespace PlantSafety.Infrastructure.Compliance.ManagementOfChange.RiskAssessments.Persistence;

/// <summary>
/// Dapper-backed repository for ManagementOfChange.RiskAssessments read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class RiskAssessmentsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<RiskAssessmentsDapperRepository> logger)
    : IRiskAssessmentsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RiskAssessmentsItemDto>> GetRiskAssessmentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRiskAssessments for ManagementOfChange.RiskAssessments tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RiskAssessmentsItemDto>(
            "[Compliance].[usp_MOCRiskAssessments_GetRiskAssessments]", parameters, cancellationToken);
    }
}
