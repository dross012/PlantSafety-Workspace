// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Infrastructure
//  File:           EnterpriseFabricReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EnterpriseFabric Application-layer read-model. Backs
//                  GetEnterpriseFabricByIdQuery. Reuses usp_EnterpriseFabricAggregate_GetById (the
//                  same proc EnterpriseFabricAggregateDapperRepository.GetByIdAsync uses to
//                  hydrate the write-side aggregate). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence
//                  .AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.EnterpriseFabric.Models;
using PlantSafety.Application.EnterpriseFabric.Repositories;

namespace PlantSafety.Infrastructure.EnterpriseFabric.Persistence;

/// <summary>Dapper-backed repository for EnterpriseFabric read-model queries.</summary>
public sealed class EnterpriseFabricReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EnterpriseFabricReadDapperRepository> logger)
    : IEnterpriseFabricRepository
{
    /// <inheritdoc/>
    public async Task<EnterpriseFabricItemDto?> GetByIdAsync(Guid enterpriseFabricId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for EnterpriseFabric {EnterpriseFabricId} Tenant {TenantId}", enterpriseFabricId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnterpriseFabricId", enterpriseFabricId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<EnterpriseFabricItemDto>(
            "[EnterpriseFabric].[usp_EnterpriseFabricAggregate_GetById]", parameters, cancellationToken);
    }
}
