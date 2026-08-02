// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Infrastructure
//  File:           AiGovernanceReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AiGovernance Application-layer read-model. Backs
//                  GetAiGovernanceByIdQuery. Reuses usp_AiGovernanceAggregate_GetById (the same
//                  proc AiGovernanceAggregateDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.AiGovernance.Models;
using PlantSafety.Application.AiGovernance.Repositories;

namespace PlantSafety.Infrastructure.AiGovernance.Persistence;

/// <summary>Dapper-backed repository for AiGovernance read-model queries.</summary>
public sealed class AiGovernanceReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AiGovernanceReadDapperRepository> logger)
    : IAiGovernanceRepository
{
    /// <inheritdoc/>
    public async Task<AiGovernanceItemDto?> GetByIdAsync(Guid aiGovernanceId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for AiGovernance {AiGovernanceId} Tenant {TenantId}", aiGovernanceId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AiGovernanceId", aiGovernanceId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<AiGovernanceItemDto>(
            "[AiGovernance].[usp_AiGovernanceAggregate_GetById]", parameters, cancellationToken);
    }
}
