// ============================================================================
//  Project:        Deployment
//  Module:         Hardening.SecretsIntegration
//  File:           SecretsAdapterContracts.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The deterministic seam Deployment
//                  uses to read/rotate secrets, deliberately an interface
//                  with no concrete vault implementation shipped here --
//                  this phase's own "no external runtime dependencies" rule
//                  means no specific secrets-vault SDK is referenced.
//                  SecretValue.Value is never written to a LogEvent Payload
//                  anywhere in this module; only SecretId/TenantId (never
//                  the secret's contents) may be logged.
// ============================================================================
namespace Deployment.Hardening.SecretsIntegration;

/// <summary>A deterministic reference to a secret -- never the secret's value.</summary>
public sealed record SecretReference(string SecretId, string TenantId);

/// <summary>A resolved secret value. Never log or serialize Value to telemetry.</summary>
public sealed record SecretValue(string SecretId, string Value, DateTime RetrievedAtUtc);

/// <summary>The deterministic seam Deployment uses to read/rotate secrets. Implemented at the composition root against a real vault, not in this module.</summary>
public interface ISecretsAdapter
{
    /// <summary>Retrieves the current value for <paramref name="reference"/>.</summary>
    Task<SecretValue> GetSecretAsync(SecretReference reference, CancellationToken cancellationToken = default);

    /// <summary>Rotates the secret identified by <paramref name="reference"/>, invalidating its prior value.</summary>
    Task RotateSecretAsync(SecretReference reference, CancellationToken cancellationToken = default);
}
