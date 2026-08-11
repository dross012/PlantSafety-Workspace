// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety
//  Layer:          Infrastructure
//  File:           HazardousMaterialsAndChemicalSafetyDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HazardousMaterialsAndChemicalSafety read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Models;
using PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Repositories;

namespace PlantSafety.Infrastructure.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Persistence;

/// <summary>Dapper-backed repository for HazardousMaterialsAndChemicalSafety read-model queries.</summary>
public sealed class HazardousMaterialsAndChemicalSafetyDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<HazardousMaterialsAndChemicalSafetyDapperRepository> logger)
    : IHazardousMaterialsAndChemicalSafetyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HazardousMaterialsAndChemicalSafetyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for HazardousMaterialsAndChemicalSafety tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HazardousMaterialsAndChemicalSafetyItemDto>(
            "[Checklists].[usp_HazardousMaterialsAndChemicalSafety_GetItems]", parameters, cancellationToken);
    }
}
