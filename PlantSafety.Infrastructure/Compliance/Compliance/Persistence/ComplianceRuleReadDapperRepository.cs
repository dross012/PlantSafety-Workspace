// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Infrastructure
//  File:           ComplianceRuleReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the (self-named) Compliance Application-layer
//                  read-model. Backs GetComplianceByIdQuery. Reuses
//                  usp_ComplianceRuleAggregate_GetById (the same proc
//                  ComplianceRuleAggregateDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate). Named "ComplianceRuleReadDapperRepository" -- not
//                  "ComplianceDapperRepository"/"ComplianceRepository" -- for the same
//                  collision-avoidance reason documented on
//                  ComplianceRuleAggregateDapperRepository. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Compliance.Models;
using PlantSafety.Application.Compliance.Compliance.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Compliance.Persistence;

/// <summary>Dapper-backed repository for Compliance read-model queries.</summary>
public sealed class ComplianceRuleReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ComplianceRuleReadDapperRepository> logger)
    : IComplianceRepository
{
    /// <inheritdoc/>
    public async Task<ComplianceItemDto?> GetByIdAsync(Guid complianceId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Compliance {ComplianceId} Tenant {TenantId}", complianceId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ComplianceId", complianceId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ComplianceItemDto>(
            "[Compliance].[usp_ComplianceRuleAggregate_GetById]", parameters, cancellationToken);
    }
}
