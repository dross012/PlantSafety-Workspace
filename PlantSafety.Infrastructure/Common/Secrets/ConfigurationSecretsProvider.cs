// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common.Secrets
//  Layer:          Infrastructure
//  File:           ConfigurationSecretsProvider.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    LOCAL-DEV-ONLY ISecretsProvider fallback reading secret values from
//                  IConfiguration (appsettings.Development.json / dotnet user-secrets / the
//                  environment). This is NOT a substitute for Azure Key Vault in any
//                  deployed environment -- it exists solely so a developer's inner loop does
//                  not require a live vault. Enforced via a startup guard: the constructor
//                  throws SecretsProviderConfigurationException if resolved while
//                  IHostEnvironment.IsProduction() is true, so this can never accidentally
//                  become the live secrets source for a production deployment. Reads secret
//                  names directly as configuration keys under the "Secrets:Values" section
//                  (e.g. "Secrets:Values:Sql-CoreDb-ConnectionString"), which is exactly
//                  where `dotnet user-secrets set "Secrets:Values:Foo" "bar"` would place a
//                  local override. Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PlantSafety.Infrastructure.Common.Secrets;

/// <summary>
/// Local-development-only <see cref="ISecretsProvider"/> reading secret values from
/// <see cref="IConfiguration"/> (typically dotnet user-secrets). Never to be selected as the
/// live secrets source in a Production environment -- see this type's constructor guard.
/// </summary>
public sealed class ConfigurationSecretsProvider : ISecretsProvider
{
    private const string ValuesSectionKey = "Secrets:Values";

    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initialises the dev-only provider. Throws <see cref="SecretsProviderConfigurationException"/>
    /// immediately if <paramref name="environment"/> reports Production -- this provider must
    /// never be constructed (let alone used) in a live production deployment.
    /// </summary>
    public ConfigurationSecretsProvider(IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsProduction())
        {
            throw new SecretsProviderConfigurationException(
                "ConfigurationSecretsProvider (the local-dev-only IConfiguration-backed secrets " +
                "fallback) was resolved while IHostEnvironment.IsProduction() is true. This " +
                "provider must NEVER back live secret access in Production -- check " +
                "InfrastructureServiceRegistration.cs's ISecretsProvider selection and/or the " +
                "'Secrets:Provider' configuration override for this environment.");
        }

        _configuration = configuration;
    }

    /// <inheritdoc/>
    public Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        var value = _configuration[$"{ValuesSectionKey}:{secretName}"];
        return Task.FromResult(value);
    }
}
