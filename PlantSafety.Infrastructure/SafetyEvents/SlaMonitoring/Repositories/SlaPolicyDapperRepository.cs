// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.SlaMonitoring
//  Layer:          Infrastructure
//  File:           SlaPolicyDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper read-side repository for SLA policy definitions (UI Gaps Scoping,
//                  IncidentManagement Part 2). All DB access via stored procedures under the
//                  [SafetyEvents] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Repositories;

namespace PlantSafety.Infrastructure.SafetyEvents.SlaMonitoring.Repositories;

/// <summary>Dapper-backed read-side repository for <c>IncidentSlaPolicy</c> rows.</summary>
public sealed class SlaPolicyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SlaPolicyDapperRepository> logger)
    : ISlaPolicyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SlaPolicyRow>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAll SLA policies for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QueryAsync<SlaPolicyRow>(
            "[SafetyEvents].[usp_IncidentSlaPolicy_GetAll]", parameters, cancellationToken);
    }
}
