// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           IWorkerWellBeingRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the WorkerWellBeing read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.WorkerWellBeing.Repositories.IWorkerWellBeingRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring
//                  PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository's
//                  identical precedent for CQRS read/write repository separation. Also distinct
//                  from the pre-existing, unrelated Features-vertical
//                  PlantSafety.Application.Features.WorkerWellBeing.Handlers.Records.IWorkerWellBeingRecordRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.WorkerWellBeing.Models;

namespace PlantSafety.Application.WorkerWellBeing.Repositories;

/// <summary>Read-only repository for WorkerWellBeing data. Implemented in the Infrastructure layer.</summary>
public interface IWorkerWellBeingRepository
{
    /// <summary>
    /// Returns the WorkerWellBeing read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<WorkerWellBeingItemDto?> GetByIdAsync(Guid workerWellBeingId, Guid tenantId, CancellationToken cancellationToken);
}
