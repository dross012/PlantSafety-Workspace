// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Export
//  Layer:          Infrastructure
//  File:           ExportDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Export read-model. All DB access
//                  via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Export.Models;
using PlantSafety.Application.Integrations.Export.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Export.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Export read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ExportDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ExportDapperRepository> logger)
    : IExportRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExportItemDto>> GetExportRunsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetExportRuns for Integrations.Export tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ExportItemDto>(
            "[Integrations].[usp_Export_GetExportRuns]", parameters, cancellationToken);
    }
}
