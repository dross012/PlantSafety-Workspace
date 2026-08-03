// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common.Secrets
//  Layer:          Infrastructure
//  File:           SecretsProviderConfigurationException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when ISecretsProvider is used in a configuration state that would
//                  otherwise silently degrade security -- e.g. AzureKeyVaultSecretsProvider
//                  invoked with no VaultUri configured, or ConfigurationSecretsProvider
//                  resolved while IHostEnvironment.IsProduction() is true. Secrets are
//                  security-critical: this subsystem deliberately fails loud rather than
//                  following the read-replica module's graceful-degrade convention.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Secrets;

/// <summary>
/// Thrown when the enterprise secrets subsystem is used in a configuration state that would
/// otherwise silently compromise security (missing vault URI, dev-only provider resolved in
/// Production, etc.).
/// </summary>
public sealed class SecretsProviderConfigurationException : InvalidOperationException
{
    /// <summary>Creates a new <see cref="SecretsProviderConfigurationException"/>.</summary>
    /// <param name="message">A clear, actionable description of the misconfiguration.</param>
    public SecretsProviderConfigurationException(string message) : base(message)
    {
    }
}
