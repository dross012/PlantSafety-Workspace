// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.IoT
//  Layer:          Infrastructure
//  File:           IoTDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.IoT read-model. All DB access
//                  via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.IoT.Models;
using PlantSafety.Application.Integrations.IoT.Repositories;

namespace PlantSafety.Infrastructure.Integrations.IoT.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.IoT read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class IoTDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<IoTDapperRepository> logger)
    : IIoTRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<IoTItemDto>> GetIoTReadingsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIoTReadings for Integrations.IoT tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<IoTItemDto>(
            "[Integrations].[usp_IoT_GetIoTReadings]", parameters, cancellationToken);
    }
}
