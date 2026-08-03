// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Configuration-driven default implementation of
//   IExternalIdentityProviderMapping (Phase 6, §2.2 "Tenant Federation").
//   No real external IdP/SSO tenant exists in this environment: this
//   implementation reads the "ExternalIdentityProviders" configuration section
//   (empty by default) and gracefully returns null when no mapping is
//   configured, exactly matching this codebase's established graceful-degrade
//   convention (see Database:ReadConnectionString / BlobStorage handling).
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlantSafety.Application.Abstractions.Identity;
using PlantSafety.Infrastructure.Common.Configuration;

namespace PlantSafety.Infrastructure.Identity;

/// <summary>
/// Resolves internal <c>TenantId</c>s from the statically configured
/// <see cref="ExternalIdentityProviderOptions"/> mapping table. Intended as the
/// production-ready default: real deployments populate the
/// <c>"ExternalIdentityProviders"</c> configuration section (typically via Azure
/// App Configuration or Key Vault-backed settings) with each federated partner's
/// issuer/tenant mapping; nothing else in this class needs to change.
/// </summary>
public sealed class ConfigurationExternalIdentityProviderMapping : IExternalIdentityProviderMapping
{
    private readonly ExternalIdentityProviderOptions _options;
    private readonly ILogger<ConfigurationExternalIdentityProviderMapping> _logger;

    /// <summary>Initialises a new instance of <see cref="ConfigurationExternalIdentityProviderMapping"/>.</summary>
    /// <param name="options">The bound external identity provider mapping configuration.</param>
    /// <param name="logger">The structured logger.</param>
    public ConfigurationExternalIdentityProviderMapping(
        IOptions<ExternalIdentityProviderOptions> options,
        ILogger<ConfigurationExternalIdentityProviderMapping> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsFederationConfigured => _options.Mappings.Count > 0;

    /// <inheritdoc />
    public Task<Guid?> ResolveTenantIdAsync(
        string externalIssuer,
        string externalTenantOrOrgClaim,
        CancellationToken cancellationToken = default)
    {
        if (!IsFederationConfigured)
        {
            // Graceful no-op: federation not configured for this environment. This is
            // NOT an error — callers (e.g. TenantResolutionMiddleware) fall back to
            // native tenantId-claim resolution, exactly as before this feature existed.
            _logger.LogDebug(
                "External identity provider federation is not configured; skipping resolution for issuer {Issuer}.",
                externalIssuer);
            return Task.FromResult<Guid?>(null);
        }

        var match = _options.Mappings.FirstOrDefault(m =>
            string.Equals(m.Issuer, externalIssuer, StringComparison.OrdinalIgnoreCase)
            && string.Equals(m.ExternalTenantOrOrgClaim, externalTenantOrOrgClaim, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            _logger.LogWarning(
                "No external identity provider mapping found for issuer {Issuer} and external tenant claim {ExternalTenantClaim}.",
                externalIssuer, externalTenantOrOrgClaim);
            return Task.FromResult<Guid?>(null);
        }

        return Task.FromResult<Guid?>(match.InternalTenantId);
    }
}
