// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.EnvironmentalMonitoring
//  Layer:          Infrastructure
//  File:           EnvironmentalMonitoringDapperRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EnvironmentalMonitoring read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.EnvironmentalMonitoring.Models;
using PlantSafety.Application.Operational.EnvironmentalMonitoring.Repositories;

namespace PlantSafety.Infrastructure.Operational.EnvironmentalMonitoring.Persistence;

/// <summary>Dapper-backed repository for <see cref="EnvironmentalMonitoringItemDto"/> queries.</summary>
public sealed class EnvironmentalMonitoringDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EnvironmentalMonitoringDapperRepository> logger)
    : IEnvironmentalMonitoringRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EnvironmentalMonitoringItemDto>> GetEnvironmentalMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEnvironmentalMonitoringAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EnvironmentalMonitoringItemDto>(
            "[Operational].[usp_EnvironmentalMonitoring_List]", parameters, cancellationToken);
    }
}