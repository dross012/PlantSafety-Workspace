// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkforceSafety
//  Layer:          Infrastructure
//  File:           WorkforceSafetyDapperRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the WorkforceSafety read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.WorkforceSafety.Models;
using PlantSafety.Application.Operational.WorkforceSafety.Repositories;

namespace PlantSafety.Infrastructure.Operational.WorkforceSafety.Persistence;

/// <summary>Dapper-backed repository for <see cref="WorkforceSafetyItemDto"/> queries.</summary>
public sealed class WorkforceSafetyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<WorkforceSafetyDapperRepository> logger)
    : IWorkforceSafetyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkforceSafetyItemDto>> GetWorkforceSafetyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetWorkforceSafetyAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<WorkforceSafetyItemDto>(
            "[Operational].[usp_WorkforceSafety_List]", parameters, cancellationToken);
    }
}