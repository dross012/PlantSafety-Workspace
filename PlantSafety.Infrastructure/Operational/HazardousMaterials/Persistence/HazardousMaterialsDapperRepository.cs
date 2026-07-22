// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.HazardousMaterials
//  Layer:          Infrastructure
//  File:           HazardousMaterialsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HazardousMaterials read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.HazardousMaterials.Models;
using PlantSafety.Application.Operational.HazardousMaterials.Repositories;

namespace PlantSafety.Infrastructure.Operational.HazardousMaterials.Persistence;

/// <summary>Dapper-backed repository for <see cref="HazardousMaterialsItemDto"/> queries.</summary>
public sealed class HazardousMaterialsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<HazardousMaterialsDapperRepository> logger)
    : IHazardousMaterialsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HazardousMaterialsItemDto>> GetHazardousMaterialsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHazardousMaterialsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HazardousMaterialsItemDto>(
            "[Operational].[usp_HazardousMaterials_List]", parameters, cancellationToken);
    }
}
