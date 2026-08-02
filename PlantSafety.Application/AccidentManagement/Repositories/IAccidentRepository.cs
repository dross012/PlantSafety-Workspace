// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-29
// FILE DESCRIPTION: Read-only repository abstraction for the Accident read-model. Implemented in
//   the Infrastructure layer. Distinct from
//   PlantSafety.Domain.AccidentManagement.Interfaces.IAccidentRepository (the write-side aggregate
//   repository) -- same name, different namespace, mirroring
//   PlantSafety.Application.RiskManagement.Repositories.IRiskRepository's identical precedent.
// SchemaVersion: 1

using PlantSafety.Application.AccidentManagement.Models;

namespace PlantSafety.Application.AccidentManagement.Repositories;

/// <summary>Read-only repository for Accident data. Implemented in the Infrastructure layer.</summary>
public interface IAccidentRepository
{
    /// <summary>
    /// Returns the Accident read-model record for the given identity, scoped to the given tenant,
    /// or <see langword="null"/> if not found or if the Accident belongs to a different tenant.
    /// </summary>
    Task<AccidentItemDto?> GetByIdAsync(Guid accidentId, Guid tenantId, CancellationToken cancellationToken);
}
