// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Infrastructure
//  File:           ProcessDefinitionReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProcessDefinition Application-layer read-model.
//                  Backs GetProcessDefinitionByIdQuery. Reuses
//                  usp_ProcessDefinitionAggregate_GetById (the same proc
//                  ProcessDefinitionAggregateDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate). Named "ProcessDefinitionReadDapperRepository" to avoid
//                  colliding with the pre-existing, unrelated Features-vertical
//                  ProcessDapperRepository in this same module folder. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ProcessManagement.Models;
using PlantSafety.Application.ProcessManagement.Repositories;

namespace PlantSafety.Infrastructure.ProcessManagement.Persistence;

/// <summary>Dapper-backed repository for ProcessDefinition read-model queries.</summary>
public sealed class ProcessDefinitionReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ProcessDefinitionReadDapperRepository> logger)
    : IProcessDefinitionRepository
{
    /// <inheritdoc/>
    public async Task<ProcessDefinitionItemDto?> GetByIdAsync(Guid processDefinitionId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for ProcessDefinition {ProcessDefinitionId} Tenant {TenantId}", processDefinitionId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProcessDefinitionId", processDefinitionId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ProcessDefinitionItemDto>(
            "[ProcessManagement].[usp_ProcessDefinitionAggregate_GetById]", parameters, cancellationToken);
    }
}
