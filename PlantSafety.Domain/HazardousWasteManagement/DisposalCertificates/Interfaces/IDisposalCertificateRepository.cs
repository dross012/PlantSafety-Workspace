// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           IDisposalCertificateRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain repository interface for the DisposalCertificate aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Interfaces;

/// <summary>Domain repository interface for the DisposalCertificate aggregate root.</summary>
public interface IDisposalCertificateRepository
{
    /// <summary>Retrieves a DisposalCertificate aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate?> GetByIdAsync(DisposalCertificateId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all DisposalCertificate aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate>> ListAsync(DisposalCertificateSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new DisposalCertificate aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing DisposalCertificate aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate aggregate, CancellationToken cancellationToken = default);
}
