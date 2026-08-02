// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Infrastructure
//  File:           CultureReadRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Culture Application-layer read-model. Backs
//                  GetCultureByIdQuery. Reuses usp_Culture_GetById (the same proc CultureRepository
//                  uses to hydrate the write-side aggregate) rather than introducing a duplicate
//                  stored procedure. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Culture.Models;
using PlantSafety.Application.Analytics.Culture.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Culture.Persistence;

/// <summary>Dapper-backed repository for Culture Application-layer read-model queries.</summary>
public sealed class CultureReadRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<CultureReadRepository> logger)
    : ICultureReadRepository
{
    /// <inheritdoc/>
    public async Task<CultureItemDto?> GetByIdAsync(Guid cultureId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Culture {CultureId} Tenant {TenantId}", cultureId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("CultureId", cultureId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CultureItemDto>(
            "[Analytics].[usp_Culture_GetById]", parameters, cancellationToken);
    }
}
