// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           ISafetyCaseValidationRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the SafetyCaseValidation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.SafetyCaseValidation"/> aggregate root.</summary>
public interface ISafetyCaseValidationRepository
{
    /// <summary>Retrieves a SafetyCaseValidation aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.SafetyCaseValidation?> GetByIdAsync(SafetyCaseValidationId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all SafetyCaseValidation aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.SafetyCaseValidation>> ListAsync(SafetyCaseValidationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new SafetyCaseValidation aggregate.</summary>
    Task AddAsync(Aggregates.SafetyCaseValidation aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing SafetyCaseValidation aggregate.</summary>
    Task UpdateAsync(Aggregates.SafetyCaseValidation aggregate, CancellationToken cancellationToken = default);
}
