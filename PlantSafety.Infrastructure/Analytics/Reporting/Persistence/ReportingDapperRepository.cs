// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Reporting
//  Layer:          Infrastructure
//  File:           ReportingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Reporting read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Reporting.Models;
using PlantSafety.Application.Analytics.Reporting.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Reporting.Persistence;

/// <summary>Dapper-backed repository for <see cref="ReportingItemDto"/> queries.</summary>
public sealed class ReportingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ReportingDapperRepository> logger)
    : IReportingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReportingItemDto>> GetReportItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetReportItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ReportingItemDto>(
            "[Analytics].[usp_Reporting_List]", parameters, cancellationToken);
    }
}
