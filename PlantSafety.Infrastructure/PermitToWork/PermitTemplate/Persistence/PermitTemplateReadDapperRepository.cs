// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Infrastructure
//  File:           PermitTemplateReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PermitTemplate Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PermitToWork.PermitTemplate.Models;
using PlantSafety.Application.PermitToWork.PermitTemplate.Repositories;

namespace PlantSafety.Infrastructure.PermitToWork.PermitTemplate.Persistence;

/// <summary>Dapper-backed repository for PermitTemplate read-model queries.</summary>
public sealed class PermitTemplateReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PermitTemplateReadDapperRepository> logger)
    : IPermitTemplateRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PermitTemplateItemDto>> GetPermitTemplateAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PermitTemplate tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PermitTemplateItemDto>(
            "[PermitToWork].[usp_PermitTemplate_GetRange]", parameters, cancellationToken);
    }
}
