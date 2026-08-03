// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Identity
//  Layer:          Domain
//  File:           IPartnerIdentityMappingRepository.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PartnerIdentityMapping
//                  aggregate root. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Identity.Aggregates;
using PlantSafety.Domain.Identity.ValueObjects;

namespace PlantSafety.Domain.Identity.Repositories;

/// <summary>Domain repository interface for the <see cref="PartnerIdentityMapping"/> aggregate root.</summary>
public interface IPartnerIdentityMappingRepository
{
    /// <summary>Retrieves a mapping by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PartnerIdentityMapping?> GetByIdAsync(PartnerIdentityMappingId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the active mapping for a partner's (issuer, external id) pair within a tenant,
    /// or <see langword="null"/> when no active mapping exists.
    /// </summary>
    /// <param name="tenantId">The tenant scope to resolve within.</param>
    /// <param name="partnerIssuer">The partner's token issuer.</param>
    /// <param name="partnerExternalId">The partner's own external identity value.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task<PartnerIdentityMapping?> FindActiveAsync(
        Guid tenantId,
        string partnerIssuer,
        string partnerExternalId,
        CancellationToken cancellationToken = default);

    /// <summary>Persists a new PartnerIdentityMapping aggregate.</summary>
    Task AddAsync(PartnerIdentityMapping aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PartnerIdentityMapping aggregate (e.g. after revocation).</summary>
    Task UpdateAsync(PartnerIdentityMapping aggregate, CancellationToken cancellationToken = default);
}
