// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing.AuditFindings
//  Layer:          Infrastructure
//  File:           AuditFindingsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AuditFindings read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Auditing.AuditFindings.Models;
using PlantSafety.Application.Compliance.Auditing.AuditFindings.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Auditing.AuditFindings.Persistence;

/// <summary>
/// Dapper-backed repository for AuditFindings read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class AuditFindingsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AuditFindingsDapperRepository> logger)
    : IAuditFindingsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditFindingItemDto>> GetFindingsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetFindings for AuditFindings tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AuditFindingItemDto>(
            "[Compliance].[usp_AuditFindings_GetFindings]", parameters, cancellationToken);
    }
}
