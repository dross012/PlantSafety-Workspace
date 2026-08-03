// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Tenant-federation abstraction (Phase 6, §2.2 "Tenant Federation").
//   Maps an external identity provider's tenant/organization claim (e.g. an Entra ID
//   external tenant GUID, an Okta "org" claim, an Auth0 "org_id" claim) onto this
//   platform's internal TenantId. Implemented by Infrastructure; consumed by
//   TenantResolutionMiddleware / CurrentUserService composition roots for
//   federated (cross-tenant, partner, B2B) sign-in flows.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

namespace PlantSafety.Application.Abstractions.Identity;

/// <summary>
/// Maps an external identity provider's tenant/organization claim to this platform's
/// internal <c>TenantId</c>. Used for federated sign-in (SSO from a partner IdP, a
/// customer's own Entra ID tenant, etc.) where the JWT's native tenant claim does not
/// itself carry a PlantSafety-recognized <c>TenantId</c> GUID.
/// </summary>
/// <remarks>
/// Zero-trust: an external claim that does not resolve to a known mapping MUST result
/// in <see langword="null"/> (never a best-guess/default tenant) — the caller (typically
/// <c>TenantResolutionMiddleware</c>) is responsible for rejecting the request (401/403)
/// when resolution fails, exactly as it already does for a missing native <c>tenantId</c>
/// claim.
/// </remarks>
public interface IExternalIdentityProviderMapping
{
    /// <summary>
    /// Resolves the internal <c>TenantId</c> corresponding to an external identity
    /// provider's issuer and tenant/organization claim value.
    /// </summary>
    /// <param name="externalIssuer">
    /// The token issuer ("iss" claim), identifying which external IdP configuration to
    /// consult (a platform may federate with more than one external IdP).
    /// </param>
    /// <param name="externalTenantOrOrgClaim">
    /// The external IdP's own tenant/organization identifier claim value (e.g. Entra ID
    /// "tid", Okta "org", Auth0 "org_id").
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>
    /// The mapped internal <c>TenantId</c>, or <see langword="null"/> when no mapping is
    /// configured for the supplied issuer/claim pair.
    /// </returns>
    Task<Guid?> ResolveTenantIdAsync(
        string externalIssuer,
        string externalTenantOrOrgClaim,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <see langword="true"/> when federation is configured at all (i.e. at least
    /// one external issuer mapping is present). Allows callers to distinguish "federation
    /// not configured for this environment" (a no-op, non-error condition — see the
    /// class-level remarks on graceful degradation) from "federation configured but this
    /// specific claim is unmapped" (a rejection condition).
    /// </summary>
    bool IsFederationConfigured { get; }
}
