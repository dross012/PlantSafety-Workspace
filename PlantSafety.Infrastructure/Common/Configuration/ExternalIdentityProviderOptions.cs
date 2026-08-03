// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           ExternalIdentityProviderOptions.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  SchemaVersion:  2026.1
//  Description:    Strongly typed configuration for tenant federation (Phase 6,
//                  §2.2). Maps external identity provider issuers to internal
//                  TenantIds. Unset (empty) by default — matches this codebase's
//                  established "optional, gracefully degrading" configuration
//                  convention (e.g. Database:ReadConnectionString). No real
//                  external IdP tenant is configured in this environment.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Configuration;

/// <summary>
/// Strongly typed configuration for external identity provider → internal
/// <c>TenantId</c> federation mappings. Bound from the <c>"ExternalIdentityProviders"</c>
/// configuration section, which is absent/empty by default.
/// </summary>
public sealed class ExternalIdentityProviderOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "ExternalIdentityProviders";

    /// <summary>
    /// Zero or more configured external IdP tenant mappings. Empty by default —
    /// federation gracefully no-ops (every resolution attempt returns
    /// <see langword="null"/>) until at least one mapping is configured.
    /// </summary>
    public IReadOnlyList<ExternalIdentityProviderMappingEntry> Mappings { get; init; }
        = Array.Empty<ExternalIdentityProviderMappingEntry>();
}

/// <summary>
/// A single external issuer → internal tenant mapping entry.
/// </summary>
public sealed class ExternalIdentityProviderMappingEntry
{
    /// <summary>The external token issuer ("iss" claim) this mapping applies to.</summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>The external IdP's own tenant/organization claim value (e.g. Entra ID "tid").</summary>
    public string ExternalTenantOrOrgClaim { get; init; } = string.Empty;

    /// <summary>The internal PlantSafety <c>TenantId</c> this external tenant/org maps to.</summary>
    public Guid InternalTenantId { get; init; }
}
