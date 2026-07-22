// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Infrastructure
//  File:           MachineProcessMonitoringReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the MachineProcessMonitoring Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Observability.MachineProcessMonitoring.Models;
using PlantSafety.Application.Observability.MachineProcessMonitoring.Repositories;

namespace PlantSafety.Infrastructure.Observability.MachineProcessMonitoring.Persistence;

/// <summary>Dapper-backed repository for MachineProcessMonitoring read-model queries.</summary>
public sealed class MachineProcessMonitoringReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<MachineProcessMonitoringReadDapperRepository> logger)
    : IMachineProcessMonitoringRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<MachineProcessMonitoringItemDto>> GetMachineProcessMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for MachineProcessMonitoring tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MachineProcessMonitoringItemDto>(
            "[Observability].[usp_MachineProcessMonitoring_GetRange]", parameters, cancellationToken);
    }
}
