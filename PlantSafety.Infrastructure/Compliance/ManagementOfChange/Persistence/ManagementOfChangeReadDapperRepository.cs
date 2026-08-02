// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Infrastructure
//  File:           ManagementOfChangeReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ManagementOfChange Application-layer read-model.
//                  Backs GetManagementOfChangeByIdQuery. Reuses usp_ManagementOfChange_GetById (the
//                  same proc ManagementOfChangeDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate) rather than introducing a duplicate stored procedure.
//                  Mirrors PermitsReadDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.ManagementOfChange.Models;
using PlantSafety.Application.Compliance.ManagementOfChange.Repositories;

namespace PlantSafety.Infrastructure.Compliance.ManagementOfChange.Persistence;

/// <summary>Dapper-backed repository for ManagementOfChange read-model queries.</summary>
public sealed class ManagementOfChangeReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ManagementOfChangeReadDapperRepository> logger)
    : IManagementOfChangeRepository
{
    /// <inheritdoc/>
    public async Task<ManagementOfChangeItemDto?> GetByIdAsync(Guid managementOfChangeId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for ManagementOfChange {ManagementOfChangeId} Tenant {TenantId}", managementOfChangeId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ManagementOfChangeId", managementOfChangeId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ManagementOfChangeItemDto>(
            "[Compliance].[usp_ManagementOfChange_GetById]", parameters, cancellationToken);
    }
}
