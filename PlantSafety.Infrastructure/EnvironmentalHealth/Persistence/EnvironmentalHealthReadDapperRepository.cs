// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Infrastructure
//  File:           EnvironmentalHealthReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EnvironmentalHealth Application-layer read-model.
//                  Backs GetEnvironmentalHealthByIdQuery. Reuses
//                  usp_EnvironmentalHealthAggregate_GetById (the same proc
//                  EnvironmentalHealthAggregateDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.EnvironmentalHealth.Models;
using PlantSafety.Application.EnvironmentalHealth.Repositories;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.Persistence;

/// <summary>Dapper-backed repository for EnvironmentalHealth read-model queries.</summary>
public sealed class EnvironmentalHealthReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EnvironmentalHealthReadDapperRepository> logger)
    : IEnvironmentalHealthRepository
{
    /// <inheritdoc/>
    public async Task<EnvironmentalHealthItemDto?> GetByIdAsync(Guid environmentalHealthId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for EnvironmentalHealth {EnvironmentalHealthId} Tenant {TenantId}", environmentalHealthId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnvironmentalHealthId", environmentalHealthId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<EnvironmentalHealthItemDto>(
            "[EnvironmentalHealth].[usp_EnvironmentalHealthAggregate_GetById]", parameters, cancellationToken);
    }
}
