// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           IComplianceRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Compliance aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Interfaces;

/// <summary>Domain repository interface for the <see cref="Compliance"/> aggregate root.</summary>
public interface IComplianceRepository
{
    /// <summary>Retrieves a Compliance aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Compliance?> GetByIdAsync(ComplianceId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Compliance aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Compliance>> ListAsync(ComplianceSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Compliance aggregate.</summary>
    Task AddAsync(Compliance aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Compliance aggregate.</summary>
    Task UpdateAsync(Compliance aggregate, CancellationToken cancellationToken = default);
}
