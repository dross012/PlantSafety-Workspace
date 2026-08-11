// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing.InternalSafetyAudit
//  Layer:          Infrastructure
//  File:           InternalSafetyAuditDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the InternalSafetyAudit read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Models;
using PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Auditing.InternalSafetyAudit.Persistence;

/// <summary>
/// Dapper-backed repository for InternalSafetyAudit read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class InternalSafetyAuditDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<InternalSafetyAuditDapperRepository> logger)
    : IInternalSafetyAuditRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<InternalSafetyAuditItemDto>> GetAuditsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAudits for InternalSafetyAudit tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<InternalSafetyAuditItemDto>(
            "[Compliance].[usp_InternalSafetyAudit_GetAudits]", parameters, cancellationToken);
    }
}
