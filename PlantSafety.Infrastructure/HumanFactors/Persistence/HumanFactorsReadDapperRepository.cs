// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Infrastructure
//  File:           HumanFactorsReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HumanFactors Application-layer read-model. Backs
//                  GetHumanFactorsByIdQuery. Reuses usp_HumanFactorsAggregate_GetById (the same
//                  proc HumanFactorsAggregateDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.HumanFactors.Models;
using PlantSafety.Application.HumanFactors.Repositories;

namespace PlantSafety.Infrastructure.HumanFactors.Persistence;

/// <summary>Dapper-backed repository for HumanFactors read-model queries.</summary>
public sealed class HumanFactorsReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<HumanFactorsReadDapperRepository> logger)
    : IHumanFactorsRepository
{
    /// <inheritdoc/>
    public async Task<HumanFactorsItemDto?> GetByIdAsync(Guid humanFactorsId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for HumanFactors {HumanFactorsId} Tenant {TenantId}", humanFactorsId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HumanFactorsId", humanFactorsId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<HumanFactorsItemDto>(
            "[HumanFactors].[usp_HumanFactorsAggregate_GetById]", parameters, cancellationToken);
    }
}
