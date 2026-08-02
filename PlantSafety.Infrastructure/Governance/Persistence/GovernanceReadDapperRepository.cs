// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Infrastructure
//  File:           GovernanceReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Governance Application-layer read-model. Backs
//                  GetGovernanceByIdQuery. Reuses usp_GovernanceAggregate_GetById (the same proc
//                  GovernanceAggregateDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Governance.Models;
using PlantSafety.Application.Governance.Repositories;

namespace PlantSafety.Infrastructure.Governance.Persistence;

/// <summary>Dapper-backed repository for Governance read-model queries.</summary>
public sealed class GovernanceReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<GovernanceReadDapperRepository> logger)
    : IGovernanceRepository
{
    /// <inheritdoc/>
    public async Task<GovernanceItemDto?> GetByIdAsync(Guid governanceId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Governance {GovernanceId} Tenant {TenantId}", governanceId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("GovernanceId", governanceId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<GovernanceItemDto>(
            "[Governance].[usp_GovernanceAggregate_GetById]", parameters, cancellationToken);
    }
}
