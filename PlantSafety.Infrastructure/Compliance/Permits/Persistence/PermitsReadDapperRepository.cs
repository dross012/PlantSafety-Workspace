// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Infrastructure
//  File:           PermitsReadDapperRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits Application-layer read-model. Backs
//                  GetPermitsByIdQuery and GetPermitsQuery. Reuses usp_Permits_GetById (the same
//                  proc PermitsDapperRepository.GetByIdAsync uses to hydrate the write-side
//                  aggregate) and usp_Permits_GetRange rather than introducing duplicate stored
//                  procedures. Mirrors AccidentReadDapperRepository.cs / RiskReadDapperRepository /
//                  PermitWorkflowReadDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.Models;
using PlantSafety.Application.Compliance.Permits.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.Persistence;

/// <summary>Dapper-backed repository for Permits read-model queries.</summary>
public sealed class PermitsReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PermitsReadDapperRepository> logger)
    : IPermitsRepository
{
    /// <inheritdoc/>
    public async Task<PermitsItemDto?> GetByIdAsync(Guid permitsId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Permits {PermitsId} Tenant {TenantId}", permitsId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitsId", permitsId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PermitsItemDto>(
            "[Compliance].[usp_Permits_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PermitsItemDto>> GetPermitsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for Permits tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PermitsItemDto>(
            "[Compliance].[usp_Permits_GetRange]", parameters, cancellationToken);
    }
}
