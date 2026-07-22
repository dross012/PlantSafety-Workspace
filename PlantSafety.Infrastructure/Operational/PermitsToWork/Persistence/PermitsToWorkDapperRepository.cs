// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PermitsToWork
//  Layer:          Infrastructure
//  File:           PermitsToWorkDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PermitsToWork read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PermitsToWork.Models;
using PlantSafety.Application.Operational.PermitsToWork.Repositories;

namespace PlantSafety.Infrastructure.Operational.PermitsToWork.Persistence;

/// <summary>Dapper-backed repository for <see cref="PermitsToWorkItemDto"/> queries.</summary>
public sealed class PermitsToWorkDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PermitsToWorkDapperRepository> logger)
    : IPermitsToWorkRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PermitsToWorkItemDto>> GetPermitsToWorkAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPermitsToWorkAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PermitsToWorkItemDto>(
            "[Operational].[usp_PermitsToWork_List]", parameters, cancellationToken);
    }
}
