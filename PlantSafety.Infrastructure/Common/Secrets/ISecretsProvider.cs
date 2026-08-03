// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common.Secrets
//  Layer:          Infrastructure
//  File:           ISecretsProvider.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 6.6 (Enterprise Deployment & Release Architecture, §6.3 Enterprise
//                  Secrets Management) canonical abstraction over "where a secret value comes
//                  from". Two implementations exist: AzureKeyVaultSecretsProvider (the
//                  production-grade implementation, backed by the real
//                  Azure.Security.KeyVault.Secrets SDK) and ConfigurationSecretsProvider (a
//                  local-dev-only fallback reading from IConfiguration/user-secrets). Callers
//                  never construct either implementation directly -- resolve
//                  ISecretsProvider from DI, which selects the right implementation per
//                  InfrastructureServiceRegistration.cs and wraps it in
//                  AuditingSecretsProviderDecorator so every retrieval is logged. Secrets are
//                  security-critical: unlike other Infrastructure abstractions in this
//                  codebase that gracefully degrade (e.g. the read-replica routing
//                  convention), a misconfigured secrets provider MUST fail loud, not silently
//                  return null. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Secrets;

/// <summary>
/// Canonical abstraction over enterprise secret retrieval. Implementations resolve a named
/// secret to its current value from whichever backing store the deployment environment
/// requires (Azure Key Vault in Production; local configuration/user-secrets in dev).
/// </summary>
public interface ISecretsProvider
{
    /// <summary>
    /// Retrieves the current value of the named secret.
    /// </summary>
    /// <param name="secretName">
    /// The logical secret name (e.g. "Sql-CoreDb-ConnectionString"). Implementations own
    /// their own naming/normalisation convention (Key Vault secret names, for instance,
    /// cannot contain certain characters that configuration keys can).
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The secret's current value, or <see langword="null"/> if no secret with that name
    /// exists in the backing store. A missing secret is a normal, non-exceptional outcome
    /// the caller must handle -- only a misconfigured PROVIDER (e.g. no vault URI at all)
    /// throws, per this interface's fail-loud contract.
    /// </returns>
    Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken);
}
