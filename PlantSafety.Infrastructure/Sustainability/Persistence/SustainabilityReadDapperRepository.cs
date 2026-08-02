// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Infrastructure
//  File:           SustainabilityReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Sustainability Application-layer read-model. Backs
//                  GetSustainabilityByIdQuery. Reuses usp_SustainabilityAggregate_GetById (the
//                  same proc SustainabilityAggregateDapperRepository.GetByIdAsync uses to hydrate
//                  the write-side aggregate). Named "SustainabilityReadDapperRepository" to avoid
//                  colliding with the pre-existing, unrelated Features-vertical
//                  SustainabilityDapperRepository. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Sustainability.Models;
using PlantSafety.Application.Sustainability.Repositories;

namespace PlantSafety.Infrastructure.Sustainability.Persistence;

/// <summary>Dapper-backed repository for Sustainability read-model queries.</summary>
public sealed class SustainabilityReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<SustainabilityReadDapperRepository> logger)
    : ISustainabilityRepository
{
    /// <inheritdoc/>
    public async Task<SustainabilityItemDto?> GetByIdAsync(Guid sustainabilityId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Sustainability {SustainabilityId} Tenant {TenantId}", sustainabilityId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SustainabilityId", sustainabilityId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<SustainabilityItemDto>(
            "[Sustainability].[usp_SustainabilityAggregate_GetById]", parameters, cancellationToken);
    }
}
