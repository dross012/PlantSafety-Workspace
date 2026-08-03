// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Default ISamlAuthenticationProvider implementation
//   (Phase 6, §2.1 "SSO Integration" — SAML). No SAML2 library is referenced
//   anywhere in this solution today, and per the Handbook's guidance this task
//   deliberately does not add one. This class is the documented "bring your
//   own SAML library" extension seam: it is a clear, fail-fast placeholder,
//   NOT a silent no-op — every method throws a descriptive
//   InvalidOperationException naming exactly what is missing and where to
//   plug in a real implementation, so a caller reaches an actionable error
//   instead of a confusing null-reference or silently-broken SSO flow.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Authentication;

namespace PlantSafety.Infrastructure.Identity;

/// <summary>
/// Default, unconfigured <see cref="ISamlAuthenticationProvider"/>. Every operation
/// throws <see cref="InvalidOperationException"/> with a message pointing at the
/// extension seam, because SAML SSO is a security-sensitive flow: silently
/// returning empty/fake data would be worse than a clear, immediate failure.
/// </summary>
/// <remarks>
/// To add real SAML2 support: reference a SAML2 library in
/// <c>PlantSafety.Infrastructure</c> (e.g. Sustainsys.Saml2, ITfoxtec.Identity.Saml2,
/// or ComponentSpace.Saml2 — none is currently referenced by this solution),
/// implement <see cref="ISamlAuthenticationProvider"/> against it, and replace this
/// class's DI registration in <c>InfrastructureServiceRegistration</c>. No other
/// layer needs to change — <see cref="ISamlAuthenticationProvider"/> is the entire
/// contract the rest of the platform depends on.
/// </remarks>
public sealed class UnconfiguredSamlAuthenticationProvider : ISamlAuthenticationProvider
{
    private const string ExtensionSeamMessage =
        "SAML2 SSO is not configured for this deployment. No SAML2 library is referenced " +
        "by PlantSafety.Infrastructure. To enable SAML SSO, implement " +
        "PlantSafety.Application.Abstractions.Authentication.ISamlAuthenticationProvider " +
        "against a SAML2 library of your choice and replace the " +
        "UnconfiguredSamlAuthenticationProvider DI registration in " +
        "InfrastructureServiceRegistration.AddInfrastructure().";

    /// <inheritdoc />
    public bool IsConfigured => false;

    /// <inheritdoc />
    public Task<string> GetServiceProviderMetadataAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(ExtensionSeamMessage);

    /// <inheritdoc />
    public Task<string> BuildAuthnRequestUrlAsync(string relayState, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(ExtensionSeamMessage);

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, string>> ValidateResponseAsync(
        string encodedSamlResponse,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(ExtensionSeamMessage);
}
