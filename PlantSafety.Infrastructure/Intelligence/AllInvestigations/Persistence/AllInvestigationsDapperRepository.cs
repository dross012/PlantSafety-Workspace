// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AllInvestigations
//  Layer:          Infrastructure
//  File:           AllInvestigationsDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AllInvestigations read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.AllInvestigations.Models;
using PlantSafety.Application.Intelligence.AllInvestigations.Repositories;

namespace PlantSafety.Infrastructure.Intelligence.AllInvestigations.Persistence;

/// <summary>
/// Dapper-backed repository for AllInvestigations read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class AllInvestigationsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AllInvestigationsDapperRepository> logger)
    : IAllInvestigationsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AllInvestigationsItemDto>> GetInvestigationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetInvestigationsAsync for AllInvestigations tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AllInvestigationsItemDto>(
            "[Intelligence].[usp_AllInvestigations_GetInvestigations]", parameters, cancellationToken);
    }
}
