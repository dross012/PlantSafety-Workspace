// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards.Supervisor
//  Layer:          Infrastructure
//  File:           SupervisorDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Supervisor read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.ScoreCards.Supervisor.Models;
using PlantSafety.Application.Analytics.ScoreCards.Supervisor.Repositories;

namespace PlantSafety.Infrastructure.Analytics.ScoreCards.Supervisor.Persistence;

/// <summary>Dapper-backed repository for <see cref="SupervisorItemDto"/> queries.</summary>
public sealed class SupervisorDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<SupervisorDapperRepository> logger)
    : ISupervisorRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SupervisorItemDto>> GetSupervisorScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSupervisorScoresAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SupervisorItemDto>(
            "[Analytics].[usp_Supervisor_List]", parameters, cancellationToken);
    }
}
