// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Reporting
//  Layer:          Infrastructure
//  File:           ReportingDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Reporting read-model. All DB access
//                  via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Reporting.Models;
using PlantSafety.Application.Integrations.Reporting.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Reporting.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Reporting read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ReportingDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ReportingDapperRepository> logger)
    : IReportingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReportingItemDto>> GetReportDeliveriesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetReportDeliveries for Integrations.Reporting tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ReportingItemDto>(
            "[Integrations].[usp_Reporting_GetReportDeliveries]", parameters, cancellationToken);
    }
}
