// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Infrastructure
//  File:           PPETrainingFitReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PPETrainingFit Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PPE.PPETrainingFit.Models;
using PlantSafety.Application.PPE.PPETrainingFit.Repositories;

namespace PlantSafety.Infrastructure.PPE.PPETrainingFit.Persistence;

/// <summary>Dapper-backed repository for PPETrainingFit read-model queries.</summary>
public sealed class PPETrainingFitReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PPETrainingFitReadDapperRepository> logger)
    : IPPETrainingFitRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PPETrainingFitItemDto>> GetPPETrainingFitAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PPETrainingFit tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PPETrainingFitItemDto>(
            "[PPE].[usp_PPETrainingFit_GetRange]", parameters, cancellationToken);
    }
}
