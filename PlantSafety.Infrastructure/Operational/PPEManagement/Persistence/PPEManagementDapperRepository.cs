// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Infrastructure
//  File:           PPEManagementDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PPEManagement read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PPEManagement.Models;
using PlantSafety.Application.Operational.PPEManagement.Repositories;

namespace PlantSafety.Infrastructure.Operational.PPEManagement.Persistence;

/// <summary>Dapper-backed repository for <see cref="PPEManagementItemDto"/> queries.</summary>
public sealed class PPEManagementDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PPEManagementDapperRepository> logger)
    : IPPEManagementRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PPEManagementItemDto>> GetPPEManagementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPPEManagementAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PPEManagementItemDto>(
            "[Operational].[usp_PPEManagement_List]", parameters, cancellationToken);
    }
}
