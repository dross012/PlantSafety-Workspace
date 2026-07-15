// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.ChemicalHandling
//  Layer:          Infrastructure
//  File:           ChemicalHandlingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.ChemicalHandling read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.ChemicalHandling.Models;
using PlantSafety.Application.Compliance.Permits.ChemicalHandling.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.ChemicalHandling.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.ChemicalHandling read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ChemicalHandlingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ChemicalHandlingDapperRepository> logger)
    : IChemicalHandlingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChemicalHandlingItemDto>> GetChemicalHandlingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetChemicalHandling for Permits.ChemicalHandling tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ChemicalHandlingItemDto>(
            "[Compliance].[usp_PermitChemicalHandling_GetChemicalHandling]", parameters, cancellationToken);
    }
}
