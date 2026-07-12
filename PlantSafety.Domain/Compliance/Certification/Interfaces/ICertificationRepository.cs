// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           ICertificationRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Certification aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Interfaces;

/// <summary>Domain repository interface for the <see cref="Certification"/> aggregate root.</summary>
public interface ICertificationRepository
{
    /// <summary>Retrieves a Certification aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Compliance.Certification.Aggregates.Certification?> GetByIdAsync(CertificationId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Certification aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Compliance.Certification.Aggregates.Certification>> ListAsync(CertificationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Certification aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Certification aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification aggregate, CancellationToken cancellationToken = default);
}
