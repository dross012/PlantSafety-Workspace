// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.FatigueManagement
//  Layer:          Infrastructure
//  File:           FatigueManagementDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the FatigueManagement read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.FatigueManagement.Models;
using PlantSafety.Application.Operational.FatigueManagement.Repositories;

namespace PlantSafety.Infrastructure.Operational.FatigueManagement.Persistence;

/// <summary>Dapper-backed repository for <see cref="FatigueManagementItemDto"/> queries.</summary>
public sealed class FatigueManagementDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<FatigueManagementDapperRepository> logger)
    : IFatigueManagementRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<FatigueManagementItemDto>> GetFatigueManagementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetFatigueManagementAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<FatigueManagementItemDto>(
            "[Operational].[usp_FatigueManagement_List]", parameters, cancellationToken);
    }
}
