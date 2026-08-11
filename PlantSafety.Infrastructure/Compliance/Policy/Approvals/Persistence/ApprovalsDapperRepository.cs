// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy.Approvals
//  Layer:          Infrastructure
//  File:           ApprovalsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Policy.Approvals read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Approvals.Models;
using PlantSafety.Application.Compliance.Policy.Approvals.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.Approvals.Persistence;

/// <summary>
/// Dapper-backed repository for Policy.Approvals read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ApprovalsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ApprovalsDapperRepository> logger)
    : IApprovalsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ApprovalsItemDto>> GetApprovalsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetApprovals for Policy.Approvals tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ApprovalsItemDto>(
            "[Compliance].[usp_PolicyApprovals_GetApprovals]", parameters, cancellationToken);
    }
}
