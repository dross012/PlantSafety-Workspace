// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           IPPEAssignmentRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PPEAssignment aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Interfaces;

/// <summary>Domain repository interface for the <see cref="PPEAssignment"/> aggregate root.</summary>
public interface IPPEAssignmentRepository
{
    /// <summary>Retrieves a PPEAssignment aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment?> GetByIdAsync(PPEAssignmentId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PPEAssignment aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment>> ListAsync(PPEAssignmentSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PPEAssignment aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PPEAssignment aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment aggregate, CancellationToken cancellationToken = default);
}