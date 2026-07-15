// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.InspectionsEquipment
//  Layer:          Infrastructure
//  File:           InspectionsEquipmentDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the InspectionsEquipment read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.Equipment.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.Equipment.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Inspections.Equipment.Persistence;

/// <summary>Dapper-backed repository for <see cref="EquipmentItemDto"/> queries.</summary>
public sealed class InspectionsEquipmentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<InspectionsEquipmentDapperRepository> logger)
    : IEquipmentRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EquipmentItemDto>> GetInspectionsAsync(
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

        return await queryExecutor.QueryAsync<EquipmentItemDto>(
            "[Compliance].[usp_InspectionsEquipment_List]", parameters, cancellationToken);
    }
}