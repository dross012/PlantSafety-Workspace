// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement.Efficiency
//  Layer:          Infrastructure
//  File:           EfficiencyDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Efficiency read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Models;
using PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditEngagement.Efficiency.Persistence;

/// <summary>Dapper-backed repository for Efficiency read-model queries.</summary>
public sealed class EfficiencyDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EfficiencyDapperRepository> logger)
    : IEfficiencyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EfficiencyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for Efficiency tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EfficiencyItemDto>(
            "[Auditing].[usp_Efficiency_GetItems]", parameters, cancellationToken);
    }
}
