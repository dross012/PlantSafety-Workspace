// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: SAML2 Service Provider abstraction (Phase 6, §2.1 "SSO Integration").
//   This is a "bring your own SAML library" extension seam, deliberately defined
//   without referencing any concrete SAML implementation package. No SAML library
//   (e.g. Sustainsys.Saml2, ITfoxtec.Identity.Saml2) is currently referenced anywhere
//   in the PlantSafety solution; per the Handbook's "don't add a new heavyweight
//   dependency without justification" convention, none is added here. The default
//   registered implementation (Infrastructure.Identity.UnconfiguredSamlAuthenticationProvider)
//   is a clear, documented no-op that fails fast with an actionable error message rather
//   than silently no-op-ing a security-relevant flow — see that class's remarks for the
//   full rationale and the extension point a future SAML integration should implement.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

namespace PlantSafety.Application.Abstractions.Authentication;

/// <summary>
/// Abstraction for a SAML2 Service Provider (SP) integration. Defines the minimal
/// surface a real SAML implementation needs to expose to plug into PlantSafety's
/// authentication pipeline, without this solution taking a compile-time dependency
/// on any specific SAML library.
/// </summary>
/// <remarks>
/// No SAML library is referenced by this solution today (verified: no
/// Sustainsys.Saml2, ITfoxtec.Identity.Saml2, ComponentSpace.Saml2, or similar
/// package reference exists anywhere in the tree). Per Phase 6 §2.1 guidance, this
/// interface is the intentional "bring your own SAML library" extension seam:
/// a future integration adds the chosen SAML NuGet package to
/// <c>PlantSafety.Infrastructure</c> only, implements this interface there, and
/// replaces the DI registration of
/// <c>UnconfiguredSamlAuthenticationProvider</c> — no other layer changes.
/// </remarks>
public interface ISamlAuthenticationProvider
{
    /// <summary>
    /// Returns <see langword="true"/> when a SAML identity provider is configured for
    /// the current environment (i.e. a real implementation is registered and has valid
    /// IdP metadata/certificates configured). Always <see langword="false"/> for the
    /// default no-op implementation.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Builds this platform's Service Provider metadata XML document (SP-initiated SSO
    /// descriptor), for the partner/customer IdP administrator to consume when
    /// configuring the trust relationship.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The SP metadata XML as a string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown by the default implementation when no SAML library/configuration is
    /// present — see <see cref="IsConfigured"/>.
    /// </exception>
    Task<string> GetServiceProviderMetadataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a SAML2 AuthnRequest redirect/POST binding target URL for SP-initiated
    /// single sign-on against the configured IdP.
    /// </summary>
    /// <param name="relayState">Opaque state round-tripped back to the SP after IdP sign-in.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The fully-formed redirect URL (or POST target) to send the browser to.</returns>
    Task<string> BuildAuthnRequestUrlAsync(string relayState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a SAML2 Response (the IdP's assertion, typically posted back to the
    /// SP's Assertion Consumer Service endpoint) and, when valid, returns the resulting
    /// claims-based identity.
    /// </summary>
    /// <param name="encodedSamlResponse">The base64-encoded SAMLResponse form field value.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The validated claim set asserted by the IdP.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown by the default implementation when no SAML library/configuration is
    /// present — see <see cref="IsConfigured"/>.
    /// </exception>
    Task<IReadOnlyDictionary<string, string>> ValidateResponseAsync(
        string encodedSamlResponse,
        CancellationToken cancellationToken = default);
}
