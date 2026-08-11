// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement.Inspecting
//  Layer:          Infrastructure
//  File:           InspectingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Inspecting read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Models;
using PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditEngagement.Inspecting.Persistence;

/// <summary>Dapper-backed repository for Inspecting read-model queries.</summary>
public sealed class InspectingDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<InspectingDapperRepository> logger)
    : IInspectingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<InspectingItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for Inspecting tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<InspectingItemDto>(
            "[Auditing].[usp_Inspecting_GetItems]", parameters, cancellationToken);
    }
}
