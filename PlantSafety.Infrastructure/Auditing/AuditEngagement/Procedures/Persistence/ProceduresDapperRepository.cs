// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement.Procedures
//  Layer:          Infrastructure
//  File:           ProceduresDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Procedures read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditEngagement.Procedures.Models;
using PlantSafety.Application.Auditing.AuditEngagement.Procedures.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditEngagement.Procedures.Persistence;

/// <summary>Dapper-backed repository for Procedures read-model queries.</summary>
public sealed class ProceduresDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ProceduresDapperRepository> logger)
    : IProceduresRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProceduresItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for Procedures tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ProceduresItemDto>(
            "[Auditing].[usp_Procedures_GetItems]", parameters, cancellationToken);
    }
}
