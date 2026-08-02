// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Infrastructure
//  File:           CertificationReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification Application-layer read-model. Backs
//                  GetCertificationByIdQuery. Reuses usp_CertificationAggregate_GetById (the same
//                  proc CertificationDapperRepository.GetByIdAsync uses to hydrate the write-side
//                  aggregate). Named "CertificationReadDapperRepository" (not
//                  "CertificationRepository") to avoid colliding with the pre-existing, unrelated
//                  dead CertificationRepository/CertificationReadModel scaffold in this same
//                  namespace. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.Models;
using PlantSafety.Application.Compliance.Certification.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Certification.Persistence;

/// <summary>Dapper-backed repository for Certification read-model queries.</summary>
public sealed class CertificationReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<CertificationReadDapperRepository> logger)
    : ICertificationRepository
{
    /// <inheritdoc/>
    public async Task<CertificationItemDto?> GetByIdAsync(Guid certificationId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Certification {CertificationId} Tenant {TenantId}", certificationId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CertificationId", certificationId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CertificationItemDto>(
            "[Compliance].[usp_CertificationAggregate_GetById]", parameters, cancellationToken);
    }
}
