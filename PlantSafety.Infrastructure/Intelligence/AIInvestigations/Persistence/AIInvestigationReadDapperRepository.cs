// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Infrastructure
//  File:           AIInvestigationReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IAIInvestigationReadRepository
//                  (Application-layer read-side repository for the AIInvestigation aggregate's
//                  by-id detail query). Reuses usp_AIInvestigation_GetById directly, avoiding
//                  domain-aggregate reconstruction. Mirrors
//                  PlantSafety.Infrastructure.SafetyEvents.RootCauseAnalysis.Persistence
//                  .RootCauseAnalysisDapperRepository's read-side precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.AIInvestigations.Models;
using PlantSafety.Application.Intelligence.AIInvestigations.Repositories;

namespace PlantSafety.Infrastructure.Intelligence.AIInvestigations.Persistence;

/// <summary>Dapper-backed read-side repository for the AIInvestigation aggregate's by-id detail query.</summary>
public sealed class AIInvestigationReadDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<AIInvestigationReadDapperRepository> logger)
    : IAIInvestigationReadRepository
{
    /// <inheritdoc/>
    public async Task<AIInvestigationDetailDto?> GetByIdAsync(Guid aiInvestigationId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for AIInvestigation {AIInvestigationId} Tenant {TenantId}", aiInvestigationId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AIInvestigationId", aiInvestigationId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<AIInvestigationDetailDto>(
            "[Intelligence].[usp_AIInvestigation_GetById]", parameters, cancellationToken);
    }
}
