// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Infrastructure
//  File:           AutomatedIncidentDetectionReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AutomatedIncidentDetection Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Observability.AutomatedIncidentDetection.Models;
using PlantSafety.Application.Observability.AutomatedIncidentDetection.Repositories;

namespace PlantSafety.Infrastructure.Observability.AutomatedIncidentDetection.Persistence;

/// <summary>Dapper-backed repository for AutomatedIncidentDetection read-model queries.</summary>
public sealed class AutomatedIncidentDetectionReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AutomatedIncidentDetectionReadDapperRepository> logger)
    : IAutomatedIncidentDetectionRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AutomatedIncidentDetectionItemDto>> GetAutomatedIncidentDetectionAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for AutomatedIncidentDetection tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AutomatedIncidentDetectionItemDto>(
            "[Observability].[usp_AutomatedIncidentDetection_GetRange]", parameters, cancellationToken);
    }
}
