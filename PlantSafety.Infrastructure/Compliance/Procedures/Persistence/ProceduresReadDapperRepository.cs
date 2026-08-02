// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Infrastructure
//  File:           ProceduresReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Procedures Application-layer read-model. Backs
//                  GetProceduresByIdQuery. Reuses usp_Procedures_GetById (the same proc
//                  ProceduresAggregateDapperRepository.GetByIdAsync uses to hydrate the write-side
//                  aggregate) rather than introducing a duplicate stored procedure. Named
//                  "ProceduresReadDapperRepository" -- distinct from the pre-existing, unregistered
//                  ProceduresRepository orphan class in this same namespace. Mirrors
//                  PermitsReadDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.Models;
using PlantSafety.Application.Compliance.Procedures.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.Persistence;

/// <summary>Dapper-backed repository for Procedures read-model queries.</summary>
public sealed class ProceduresReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ProceduresReadDapperRepository> logger)
    : IProceduresRepository
{
    /// <inheritdoc/>
    public async Task<ProceduresItemDto?> GetByIdAsync(Guid proceduresId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Procedures {ProceduresId} Tenant {TenantId}", proceduresId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProceduresId", proceduresId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ProceduresItemDto>(
            "[Compliance].[usp_Procedures_GetById]", parameters, cancellationToken);
    }
}
