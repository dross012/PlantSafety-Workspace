// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Inspections
//  Layer:          Infrastructure
//  File:           InspectionsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Inspections read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Inspections.Models;
using PlantSafety.Application.Operational.Inspections.Repositories;

namespace PlantSafety.Infrastructure.Operational.Inspections.Persistence;

/// <summary>Dapper-backed repository for <see cref="InspectionsItemDto"/> queries.</summary>
public sealed class InspectionsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<InspectionsDapperRepository> logger)
    : IInspectionsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<InspectionsItemDto>> GetInspectionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetInspectionsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<InspectionsItemDto>(
            "[Operational].[usp_Inspections_List]", parameters, cancellationToken);
    }
}
