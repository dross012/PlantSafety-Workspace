// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Predictive.ComplianceFailure
//  Layer:          Infrastructure
//  File:           ComplianceFailureDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ComplianceFailure read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Predictive.ComplianceFailure.Models;
using PlantSafety.Application.Analytics.Predictive.ComplianceFailure.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Predictive.ComplianceFailure.Persistence;

/// <summary>Dapper-backed repository for <see cref="ComplianceFailureItemDto"/> queries.</summary>
public sealed class ComplianceFailureDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ComplianceFailureDapperRepository> logger)
    : IComplianceFailureRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ComplianceFailureItemDto>> GetComplianceFailurePredictionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetComplianceFailurePredictionsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ComplianceFailureItemDto>(
            "[Analytics].[usp_ComplianceFailure_List]", parameters, cancellationToken);
    }
}
