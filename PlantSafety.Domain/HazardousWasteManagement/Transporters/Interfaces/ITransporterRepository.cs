// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           ITransporterRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain repository interface for the Transporter aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.Transporters.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Interfaces;

/// <summary>Domain repository interface for the Transporter aggregate root.</summary>
public interface ITransporterRepository
{
    /// <summary>Retrieves a Transporter aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates.Transporter?> GetByIdAsync(TransporterId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Transporter aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates.Transporter>> ListAsync(TransporterSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Transporter aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates.Transporter aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Transporter aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates.Transporter aggregate, CancellationToken cancellationToken = default);
}
