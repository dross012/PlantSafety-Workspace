// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Infrastructure
//  File:           PPEAssignmentReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PPEAssignment Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PPE.PPEAssignment.Models;
using PlantSafety.Application.PPE.PPEAssignment.Repositories;

namespace PlantSafety.Infrastructure.PPE.PPEAssignment.Persistence;

/// <summary>Dapper-backed repository for PPEAssignment read-model queries.</summary>
public sealed class PPEAssignmentReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PPEAssignmentReadDapperRepository> logger)
    : IPPEAssignmentRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PPEAssignmentItemDto>> GetPPEAssignmentAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PPEAssignment tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PPEAssignmentItemDto>(
            "[PPE].[usp_PPEAssignment_GetRange]", parameters, cancellationToken);
    }
}
