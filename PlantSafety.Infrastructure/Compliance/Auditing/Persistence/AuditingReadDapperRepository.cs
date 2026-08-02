// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Infrastructure
//  File:           AuditingReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Auditing Application-layer read-model. Backs
//                  GetAuditingByIdQuery. Reuses usp_AuditingAggregate_GetById (the same proc
//                  AuditingDapperRepository.GetByIdAsync uses to hydrate the write-side
//                  aggregate). Named "AuditingReadDapperRepository" (not "AuditingRepository") to
//                  avoid colliding with the pre-existing, unrelated dead
//                  AuditingRepository/AuditingReadModel scaffold in this same namespace. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Auditing.Models;
using PlantSafety.Application.Compliance.Auditing.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Auditing.Persistence;

/// <summary>Dapper-backed repository for Auditing read-model queries.</summary>
public sealed class AuditingReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AuditingReadDapperRepository> logger)
    : IAuditingRepository
{
    /// <inheritdoc/>
    public async Task<AuditingItemDto?> GetByIdAsync(Guid auditingId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Auditing {AuditingId} Tenant {TenantId}", auditingId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AuditingId", auditingId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<AuditingItemDto>(
            "[Compliance].[usp_AuditingAggregate_GetById]", parameters, cancellationToken);
    }
}
