// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           IPublicSafetyAgencyRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PublicSafetyAgency aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Interfaces;

/// <summary>Domain repository interface for the <see cref="PublicSafetyAgency"/> aggregate root.</summary>
public interface IPublicSafetyAgencyRepository
{
    /// <summary>Retrieves a PublicSafetyAgency aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency?> GetByIdAsync(PublicSafetyAgencyId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PublicSafetyAgency aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency>> ListAsync(PublicSafetyAgencySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PublicSafetyAgency aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PublicSafetyAgency aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency aggregate, string? eTag, CancellationToken cancellationToken = default);
}
