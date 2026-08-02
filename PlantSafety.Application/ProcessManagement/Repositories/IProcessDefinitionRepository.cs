// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           IProcessDefinitionRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the ProcessDefinition read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.ProcessManagement.Repositories.IProcessDefinitionRepository
//                  (the write-side aggregate repository) -- same simple name, different
//                  namespace, mirroring
//                  PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ProcessManagement.Models;

namespace PlantSafety.Application.ProcessManagement.Repositories;

/// <summary>Read-only repository for ProcessDefinition data. Implemented in the Infrastructure layer.</summary>
public interface IProcessDefinitionRepository
{
    /// <summary>
    /// Returns the ProcessDefinition read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<ProcessDefinitionItemDto?> GetByIdAsync(Guid processDefinitionId, Guid tenantId, CancellationToken cancellationToken);
}
