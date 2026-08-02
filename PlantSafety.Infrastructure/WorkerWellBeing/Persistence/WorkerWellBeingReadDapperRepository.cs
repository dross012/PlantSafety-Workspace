// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Infrastructure
//  File:           WorkerWellBeingReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the WorkerWellBeing Application-layer read-model. Backs
//                  GetWorkerWellBeingByIdQuery. Reuses usp_WorkerWellBeingAggregate_GetById (the
//                  same proc WorkerWellBeingAggregateDapperRepository.GetByIdAsync uses to
//                  hydrate the write-side aggregate). Named
//                  "WorkerWellBeingReadDapperRepository" to avoid colliding with the
//                  pre-existing, unrelated Features-vertical WorkerWellBeingDapperRepository.
//                  Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.WorkerWellBeing.Models;
using PlantSafety.Application.WorkerWellBeing.Repositories;

namespace PlantSafety.Infrastructure.WorkerWellBeing.Persistence;

/// <summary>Dapper-backed repository for WorkerWellBeing read-model queries.</summary>
public sealed class WorkerWellBeingReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<WorkerWellBeingReadDapperRepository> logger)
    : IWorkerWellBeingRepository
{
    /// <inheritdoc/>
    public async Task<WorkerWellBeingItemDto?> GetByIdAsync(Guid workerWellBeingId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for WorkerWellBeing {WorkerWellBeingId} Tenant {TenantId}", workerWellBeingId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkerWellBeingId", workerWellBeingId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<WorkerWellBeingItemDto>(
            "[WorkerWellBeing].[usp_WorkerWellBeingAggregate_GetById]", parameters, cancellationToken);
    }
}
