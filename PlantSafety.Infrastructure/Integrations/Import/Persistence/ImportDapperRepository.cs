// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Import
//  Layer:          Infrastructure
//  File:           ImportDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Import read-model. All DB access
//                  via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Import.Models;
using PlantSafety.Application.Integrations.Import.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Import.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Import read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ImportDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ImportDapperRepository> logger)
    : IImportRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ImportItemDto>> GetImportRunsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetImportRuns for Integrations.Import tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ImportItemDto>(
            "[Integrations].[usp_Import_GetImportRuns]", parameters, cancellationToken);
    }
}
