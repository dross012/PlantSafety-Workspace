// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Infrastructure
//  File:           RiskReadDapperRepository.cs
//  Created:        2026-07-29
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Risk Application-layer read-model. Backs
//                  GetRiskByIdQuery. Reuses usp_Risk_GetById (the same proc
//                  RiskDapperRepository.GetByIdAsync uses to hydrate the write-side aggregate)
//                  rather than introducing a duplicate stored procedure -- mirrors
//                  PermitWorkflowReadDapperRepository's precedent of reusing write-side procs for
//                  Application-layer flat-DTO reads. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.RiskManagement.Models;
using PlantSafety.Application.RiskManagement.Repositories;

namespace PlantSafety.Infrastructure.RiskManagement.Persistence;

/// <summary>Dapper-backed repository for Risk read-model queries.</summary>
public sealed class RiskReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RiskReadDapperRepository> logger)
    : IRiskRepository
{
    /// <inheritdoc/>
    public async Task<RiskItemDto?> GetByIdAsync(Guid riskId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Risk {RiskId} Tenant {TenantId}", riskId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RiskId", riskId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<RiskItemDto>(
            "[RiskMgmt].[usp_Risk_GetById]", parameters, cancellationToken);
    }
}
