// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing.ComplianceAudit
//  Layer:          Infrastructure
//  File:           ComplianceAuditDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ComplianceAudit read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Models;
using PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Auditing.ComplianceAudit.Persistence;

/// <summary>
/// Dapper-backed repository for ComplianceAudit read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ComplianceAuditDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ComplianceAuditDapperRepository> logger)
    : IComplianceAuditRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ComplianceAuditItemDto>> GetAuditsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAudits for ComplianceAudit tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ComplianceAuditItemDto>(
            "[Compliance].[usp_ComplianceAudit_GetAudits]", parameters, cancellationToken);
    }
}
