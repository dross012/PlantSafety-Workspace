// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate.PersonalProtectiveEquipment
//  Layer:          Infrastructure
//  File:           PersonalProtectiveEquipmentDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PersonalProtectiveEquipment read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Models;
using PlantSafety.Application.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Repositories;

namespace PlantSafety.Infrastructure.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Persistence;

/// <summary>Dapper-backed repository for PersonalProtectiveEquipment read-model queries.</summary>
public sealed class PersonalProtectiveEquipmentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PersonalProtectiveEquipmentDapperRepository> logger)
    : IPersonalProtectiveEquipmentRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersonalProtectiveEquipmentItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for PersonalProtectiveEquipment tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PersonalProtectiveEquipmentItemDto>(
            "[Checklists].[usp_PersonalProtectiveEquipment_GetItems]", parameters, cancellationToken);
    }
}
