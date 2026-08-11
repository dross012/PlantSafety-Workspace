// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Infrastructure
//  File:           IncidentManagementDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the IncidentManagement read-model backing
//                  IIncidentManagementRepository (the legacy SafetyEvents.IncidentManagement.Repositories
//                  interface). Distinct from the pre-existing IncidentManagementRepository in this same
//                  folder, which backs the unrelated Features-tree read model.
//                  All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.IncidentManagement.Models;
using PlantSafety.Application.SafetyEvents.IncidentManagement.Repositories;

namespace PlantSafety.Infrastructure.SafetyEvents.IncidentManagement.Persistence;

/// <summary>
/// Dapper-backed repository for IncidentManagement read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class IncidentManagementDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<IncidentManagementDapperRepository> logger)
    : IIncidentManagementRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncidentManagementItemDto>> GetIncidentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIncidents for IncidentManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<IncidentManagementItemDto>(
            "[SafetyEvents].[usp_IncidentManagementItems_GetIncidents]", parameters, cancellationToken);
    }
}
