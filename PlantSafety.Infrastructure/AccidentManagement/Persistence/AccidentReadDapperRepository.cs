// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Infrastructure
//  File:           AccidentReadDapperRepository.cs
//  Created:        2026-07-29
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Accident Application-layer read-model. Backs
//                  GetAccidentByIdQuery. Reuses usp_Accident_GetById (the same proc
//                  AccidentDapperRepository.GetByIdAsync uses to hydrate the write-side aggregate)
//                  rather than introducing a duplicate stored procedure. Mirrors
//                  RiskReadDapperRepository's identical precedent. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.AccidentManagement.Models;
using PlantSafety.Application.AccidentManagement.Repositories;

namespace PlantSafety.Infrastructure.AccidentManagement.Persistence;

/// <summary>Dapper-backed repository for Accident read-model queries.</summary>
public sealed class AccidentReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AccidentReadDapperRepository> logger)
    : IAccidentRepository
{
    /// <inheritdoc/>
    public async Task<AccidentItemDto?> GetByIdAsync(Guid accidentId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Accident {AccidentId} Tenant {TenantId}", accidentId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AccidentId", accidentId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<AccidentItemDto>(
            "[AccidentManagement].[usp_Accident_GetById]", parameters, cancellationToken);
    }
}
