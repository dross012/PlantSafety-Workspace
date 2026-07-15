// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicyEquipment
//  Layer:          Infrastructure
//  File:           PolicyEquipmentDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicyEquipment read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Equipment.Models;
using PlantSafety.Application.Compliance.Policy.Equipment.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.Equipment.Persistence;

/// <summary>Dapper-backed repository for <see cref="EquipmentItemDto"/> queries.</summary>
public sealed class PolicyEquipmentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PolicyEquipmentDapperRepository> logger)
    : IEquipmentRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EquipmentItemDto>> GetEquipmentPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEquipmentPoliciesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EquipmentItemDto>(
            "[Compliance].[usp_PolicyEquipment_List]", parameters, cancellationToken);
    }
}