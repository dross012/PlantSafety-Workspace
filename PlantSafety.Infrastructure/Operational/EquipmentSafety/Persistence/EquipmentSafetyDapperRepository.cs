// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.EquipmentSafety
//  Layer:          Infrastructure
//  File:           EquipmentSafetyDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EquipmentSafety read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.EquipmentSafety.Models;
using PlantSafety.Application.Operational.EquipmentSafety.Repositories;

namespace PlantSafety.Infrastructure.Operational.EquipmentSafety.Persistence;

/// <summary>Dapper-backed repository for <see cref="EquipmentSafetyItemDto"/> queries.</summary>
public sealed class EquipmentSafetyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EquipmentSafetyDapperRepository> logger)
    : IEquipmentSafetyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EquipmentSafetyItemDto>> GetEquipmentSafetyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEquipmentSafetyAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EquipmentSafetyItemDto>(
            "[Operational].[usp_EquipmentSafety_List]", parameters, cancellationToken);
    }
}
