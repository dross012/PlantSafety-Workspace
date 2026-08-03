// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common.Secrets
//  Layer:          Infrastructure
//  File:           SecretsProviderOptions.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed configuration for ISecretsProvider selection and the Azure
//                  Key Vault implementation. Bound from configuration section
//                  "Secrets" (appsettings.json). VaultUri is intentionally unset (null) by
//                  default -- see AzureKeyVaultSecretsProvider for the fail-loud behaviour
//                  this drives. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Secrets;

/// <summary>Strongly typed configuration for the enterprise secrets subsystem.</summary>
public sealed class SecretsProviderOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Secrets";

    /// <summary>
    /// Explicit provider selection override: "AzureKeyVault" or "Configuration". When null/empty,
    /// InfrastructureServiceRegistration.cs falls back to environment-based selection
    /// (Production -&gt; AzureKeyVault, everything else -&gt; Configuration).
    /// </summary>
    public string? Provider { get; init; }

    /// <summary>
    /// The Azure Key Vault vault URI (e.g. "https://plantsafety-prod-kv.vault.azure.net/"),
    /// consumed by AzureKeyVaultSecretsProvider. Deliberately unset by default -- this is a
    /// per-environment secret store endpoint, not a value to hardcode or fabricate. If the
    /// AzureKeyVaultSecretsProvider is selected/resolved while this is unset, it throws
    /// SecretsProviderConfigurationException on first use rather than silently no-op'ing.
    /// </summary>
    public string? VaultUri { get; init; }
}
