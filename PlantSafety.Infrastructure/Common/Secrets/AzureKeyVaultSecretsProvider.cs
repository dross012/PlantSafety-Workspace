// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common.Secrets
//  Layer:          Infrastructure
//  File:           AzureKeyVaultSecretsProvider.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Production-grade ISecretsProvider implementation backed by the real
//                  Azure.Security.KeyVault.Secrets SDK (SecretClient). Authenticates via
//                  DefaultAzureCredential (Azure.Identity) so no client secret/certificate is
//                  ever hardcoded here -- in Azure-hosted environments this resolves to
//                  Managed Identity; in local dev it falls back through Azure CLI/Visual
//                  Studio/environment-variable credentials per DefaultAzureCredential's
//                  documented probe order. The SecretClient is constructed lazily on first
//                  use (not in the constructor) so that a missing VaultUri configuration
//                  fails loud with a clear, actionable exception at first real use rather
//                  than crashing DI container composition for an app that might not need
//                  secrets on every code path. This cannot be exercised against a real vault
//                  in this sandboxed dev environment -- see docs\Phase6\deployment-release-
//                  architecture.md for the honest "needs real infra" note. Additive-only
//                  evolution.
// ============================================================================
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;

namespace PlantSafety.Infrastructure.Common.Secrets;

/// <summary>
/// Production ISecretsProvider implementation over Azure Key Vault, authenticated via
/// <see cref="DefaultAzureCredential"/>.
/// </summary>
public sealed class AzureKeyVaultSecretsProvider : ISecretsProvider
{
    private readonly SecretsProviderOptions _options;
    private readonly object _clientLock = new();
    private SecretClient? _client;

    /// <summary>Initialises the provider with its configuration-driven vault URI.</summary>
    public AzureKeyVaultSecretsProvider(IOptions<SecretsProviderOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        var client = GetOrCreateClient();

        try
        {
            var response = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return response.Value.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // A secret genuinely not existing in the vault is a normal outcome, not a
            // provider misconfiguration -- return null per ISecretsProvider's contract.
            return null;
        }
    }

    /// <summary>
    /// Lazily constructs (and caches) the underlying <see cref="SecretClient"/>. Throws
    /// <see cref="SecretsProviderConfigurationException"/> -- loudly, not a silent no-op --
    /// if no vault URI has been configured, since this provider must never be usable as a
    /// disguised no-op secrets source.
    /// </summary>
    private SecretClient GetOrCreateClient()
    {
        if (_client is not null)
        {
            return _client;
        }

        lock (_clientLock)
        {
            if (_client is not null)
            {
                return _client;
            }

            if (string.IsNullOrWhiteSpace(_options.VaultUri))
            {
                throw new SecretsProviderConfigurationException(
                    "AzureKeyVaultSecretsProvider was resolved but no vault URI is configured " +
                    "(configuration key 'Secrets:VaultUri'). Secrets access must fail loud rather " +
                    "than silently return no secrets -- configure the vault URI for this " +
                    "environment or select the Configuration provider explicitly for local dev " +
                    "via 'Secrets:Provider' = 'Configuration'.");
            }

            _client = new SecretClient(new Uri(_options.VaultUri), new DefaultAzureCredential());
            return _client;
        }
    }
}
