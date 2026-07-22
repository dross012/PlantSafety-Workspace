// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Analyzers
//  Layer:          Infrastructure
//  File:           AnalyzersDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Analyzers read-model. All DB access
//                  via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Analyzers.Models;
using PlantSafety.Application.Integrations.Analyzers.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Analyzers.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Analyzers read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class AnalyzersDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AnalyzersDapperRepository> logger)
    : IAnalyzersRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AnalyzersItemDto>> GetRunsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRuns for Integrations.Analyzers tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AnalyzersItemDto>(
            "[Integrations].[usp_Analyzers_GetRuns]", parameters, cancellationToken);
    }
}
