// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           IProceduresRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Procedures aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Interfaces;

/// <summary>Domain repository interface for the <see cref="Procedures"/> aggregate root.</summary>
public interface IProceduresRepository
{
    /// <summary>Retrieves a Procedures aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures?> GetByIdAsync(ProceduresId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Procedures aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures>> ListAsync(ProceduresSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Procedures aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Procedures aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures aggregate, CancellationToken cancellationToken = default);
}
