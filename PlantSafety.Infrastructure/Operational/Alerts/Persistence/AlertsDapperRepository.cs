// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Infrastructure
//  File:           AlertsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Alerts read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Alerts.Models;
using PlantSafety.Application.Operational.Alerts.Repositories;

namespace PlantSafety.Infrastructure.Operational.Alerts.Persistence;

/// <summary>Dapper-backed repository for <see cref="AlertsItemDto"/> queries.</summary>
public sealed class AlertsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<AlertsDapperRepository> logger)
    : IAlertsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AlertsItemDto>> GetAlertsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAlertsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AlertsItemDto>(
            "[Operational].[usp_Alerts_List]", parameters, cancellationToken);
    }
}
