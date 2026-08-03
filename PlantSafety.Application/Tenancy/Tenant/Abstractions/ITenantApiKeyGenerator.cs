// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Application-layer abstraction for the "generate API keys" step of the
//   automated tenant provisioning pipeline (Phase 7 §2.1). The pre-existing ApiKeyAuthFilter
//   validates a single, platform-wide, Key-Vault-backed API key (ApiSecurity:ApiKey config
//   section) -- there was no per-tenant API key issuance/storage mechanism anywhere in the
//   codebase prior to this (confirmed by search), so this is a real, new, minimal mechanism,
//   scoped to what tenant-facing machine-to-machine/webhook clients need: one primary key per
//   tenant, generated once at provisioning time and rotatable thereafter. Additive-only
//   evolution -- does not change ApiKeyAuthFilter's existing single-platform-key validation path.
// SchemaVersion: 1

namespace PlantSafety.Application.Tenancy.Tenant.Abstractions;

/// <summary>Generates and stores tenant-scoped API keys for machine-to-machine/webhook clients.</summary>
public interface ITenantApiKeyGenerator
{
    /// <summary>
    /// Generates a new, cryptographically random API key for <paramref name="tenantId"/>, persists
    /// its salted hash (never the plaintext key), and returns the plaintext key exactly once --
    /// callers must display/deliver it to the tenant immediately, it cannot be retrieved again.
    /// </summary>
    /// <param name="tenantId">The tenant the key is scoped to.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The plaintext API key, shown to the caller exactly once.</returns>
    Task<string> GenerateAsync(Guid tenantId, CancellationToken cancellationToken);
}
