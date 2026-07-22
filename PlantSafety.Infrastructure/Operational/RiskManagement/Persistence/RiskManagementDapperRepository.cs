// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Infrastructure
//  File:           RiskManagementDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RiskManagement read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.RiskManagement.Models;
using PlantSafety.Application.Operational.RiskManagement.Repositories;

namespace PlantSafety.Infrastructure.Operational.RiskManagement.Persistence;

/// <summary>Dapper-backed repository for <see cref="RiskManagementItemDto"/> queries.</summary>
public sealed class RiskManagementDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<RiskManagementDapperRepository> logger)
    : IRiskManagementRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RiskManagementItemDto>> GetRiskManagementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRiskManagementAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RiskManagementItemDto>(
            "[Operational].[usp_RiskManagement_List]", parameters, cancellationToken);
    }
}
