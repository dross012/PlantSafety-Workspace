// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           ICredentialRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Credential aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces;

/// <summary>Domain repository interface for the <see cref="Credential"/> aggregate root.</summary>
public interface ICredentialRepository
{
    /// <summary>Retrieves a Credential aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential?> GetByIdAsync(CredentialId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Credential aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential>> ListAsync(CredentialSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Credential aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Credential aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential aggregate, CancellationToken cancellationToken = default);
}
