// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Infrastructure
//  File:           PolicyReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Policy Application-layer read-model. Backs
//                  GetPolicyByIdQuery. Reuses usp_Policy_GetById (the same proc
//                  PolicyAggregateDapperRepository.GetByIdAsync uses to hydrate the write-side
//                  aggregate) rather than introducing a duplicate stored procedure. Mirrors
//                  PermitsReadDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Models;
using PlantSafety.Application.Compliance.Policy.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policies.Persistence;

/// <summary>Dapper-backed repository for Policy read-model queries.</summary>
public sealed class PolicyReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PolicyReadDapperRepository> logger)
    : IPolicyRepository
{
    /// <inheritdoc/>
    public async Task<PolicyItemDto?> GetByIdAsync(Guid policyId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Policy {PolicyId} Tenant {TenantId}", policyId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PolicyId", policyId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PolicyItemDto>(
            "[Compliance].[usp_Policy_GetById]", parameters, cancellationToken);
    }
}
