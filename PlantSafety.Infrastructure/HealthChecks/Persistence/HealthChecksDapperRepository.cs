// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HealthChecks
//  Layer:          Infrastructure
//  File:           HealthChecksDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HealthChecks read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.HealthChecks.Models;
using PlantSafety.Application.HealthChecks.Repositories;

namespace PlantSafety.Infrastructure.HealthChecks.Persistence;

/// <summary>
/// Dapper-backed repository for HealthChecks read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class HealthChecksDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<HealthChecksDapperRepository> logger)
    : IHealthChecksRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HealthCheckItemDto>> GetChecksAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetChecksAsync for HealthChecks tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HealthCheckItemDto>(
            "[HealthChecks].[usp_HealthChecks_GetChecks]", parameters, cancellationToken);
    }
}
