// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Infrastructure
//  File:           OperationalSafetyReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the OperationalSafety Application-layer read-model.
//                  Backs GetOperationalSafetyByIdQuery. Reuses
//                  usp_OperationalSafetyAggregate_GetById (the same proc
//                  OperationalSafetyAggregateDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.OperationalSafety.Models;
using PlantSafety.Application.OperationalSafety.Repositories;

namespace PlantSafety.Infrastructure.OperationalSafety.Persistence;

/// <summary>Dapper-backed repository for OperationalSafety read-model queries.</summary>
public sealed class OperationalSafetyReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<OperationalSafetyReadDapperRepository> logger)
    : IOperationalSafetyRepository
{
    /// <inheritdoc/>
    public async Task<OperationalSafetyItemDto?> GetByIdAsync(Guid operationalSafetyId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for OperationalSafety {OperationalSafetyId} Tenant {TenantId}", operationalSafetyId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OperationalSafetyId", operationalSafetyId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<OperationalSafetyItemDto>(
            "[OperationalSafety].[usp_OperationalSafetyAggregate_GetById]", parameters, cancellationToken);
    }
}
