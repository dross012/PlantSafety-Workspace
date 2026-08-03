// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Identity
//  Layer:          Domain
//  File:           PartnerIdentityMappingAlreadyExistsException.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  SchemaVersion:  2026.1
//  Description:    Thrown when attempting to register a partner identity mapping
//                  for an (Issuer, PartnerExternalId) pair already mapped within
//                  the tenant. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Identity.Exceptions;

/// <summary>
/// Thrown when a partner identity mapping is registered for a (tenant, issuer,
/// partner external id) combination that already has an active mapping.
/// </summary>
public sealed class PartnerIdentityMappingAlreadyExistsException : Exception
{
    /// <summary>The tenant the conflicting mapping belongs to.</summary>
    public Guid TenantId { get; }

    /// <summary>The partner token issuer of the conflicting mapping.</summary>
    public string PartnerIssuer { get; }

    /// <summary>The partner's external identity value of the conflicting mapping.</summary>
    public string PartnerExternalId { get; }

    /// <summary>Initialises a new instance of <see cref="PartnerIdentityMappingAlreadyExistsException"/>.</summary>
    /// <param name="tenantId">The tenant the conflicting mapping belongs to.</param>
    /// <param name="partnerIssuer">The partner token issuer of the conflicting mapping.</param>
    /// <param name="partnerExternalId">The partner's external identity value of the conflicting mapping.</param>
    public PartnerIdentityMappingAlreadyExistsException(Guid tenantId, string partnerIssuer, string partnerExternalId)
        : base($"A partner identity mapping already exists for tenant '{tenantId}', issuer '{partnerIssuer}', " +
               $"partner external id '{partnerExternalId}'.")
    {
        TenantId = tenantId;
        PartnerIssuer = partnerIssuer;
        PartnerExternalId = partnerExternalId;
    }
}
