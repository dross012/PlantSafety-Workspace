// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate.ContractorSafety
//  Layer:          Infrastructure
//  File:           ContractorSafetyDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ContractorSafety read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Models;
using PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Repositories;

namespace PlantSafety.Infrastructure.Checklists.ChecklistTemplate.ContractorSafety.Persistence;

/// <summary>Dapper-backed repository for ContractorSafety read-model queries.</summary>
public sealed class ContractorSafetyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ContractorSafetyDapperRepository> logger)
    : IContractorSafetyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContractorSafetyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for ContractorSafety tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ContractorSafetyItemDto>(
            "[Checklists].[usp_ContractorSafety_GetItems]", parameters, cancellationToken);
    }
}
