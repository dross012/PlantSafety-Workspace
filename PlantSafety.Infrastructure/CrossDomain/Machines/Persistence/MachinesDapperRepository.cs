// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Machines
//  Layer:          Infrastructure
//  File:           MachinesDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Machines read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Machines.Models;
using PlantSafety.Application.CrossDomain.Machines.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Machines.Persistence;

/// <summary>Dapper-backed repository for <see cref="MachinesItemDto"/> queries.</summary>
public sealed class MachinesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<MachinesDapperRepository> logger)
    : IMachinesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<MachinesItemDto>> GetMachinesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMachinesAsync for Machines tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MachinesItemDto>(
            "[CrossDomain].[usp_Machines_GetMachines]", parameters, cancellationToken);
    }
}
