// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           IDigitalTwinRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the DigitalTwin read-model. Implemented
//                  in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.DigitalTwin.Repositories.IDigitalTwinRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.DigitalTwin.Models;

namespace PlantSafety.Application.DigitalTwin.Repositories;

/// <summary>Read-only repository for DigitalTwin data. Implemented in the Infrastructure layer.</summary>
public interface IDigitalTwinRepository
{
    /// <summary>
    /// Returns the DigitalTwin read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<DigitalTwinItemDto?> GetByIdAsync(Guid digitalTwinId, Guid tenantId, CancellationToken cancellationToken);
}
