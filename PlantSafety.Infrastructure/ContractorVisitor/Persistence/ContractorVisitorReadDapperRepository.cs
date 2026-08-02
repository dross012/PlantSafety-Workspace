// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Infrastructure
//  File:           ContractorVisitorReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ContractorVisitor Application-layer read-model.
//                  Backs GetContractorVisitorByIdQuery. Reuses
//                  usp_ContractorVisitorAggregate_GetById. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ContractorVisitor.Models;
using PlantSafety.Application.ContractorVisitor.Repositories;

namespace PlantSafety.Infrastructure.ContractorVisitor.Persistence;

/// <summary>Dapper-backed repository for ContractorVisitor read-model queries.</summary>
public sealed class ContractorVisitorReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ContractorVisitorReadDapperRepository> logger)
    : IContractorVisitorRepository
{
    /// <inheritdoc/>
    public async Task<ContractorVisitorItemDto?> GetByIdAsync(Guid contractorVisitorId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for ContractorVisitor {ContractorVisitorId} Tenant {TenantId}", contractorVisitorId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ContractorVisitorId", contractorVisitorId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ContractorVisitorItemDto>(
            "[ContractorVisitor].[usp_ContractorVisitorAggregate_GetById]", parameters, cancellationToken);
    }
}
